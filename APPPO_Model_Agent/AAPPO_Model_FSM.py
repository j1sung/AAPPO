# 라이브러리 불러오기
import numpy as np
import datetime
import platform
import torch
import torch.nn.functional as F
from torch.utils.tensorboard import SummaryWriter
from mlagents_envs.environment import UnityEnvironment, ActionTuple
from mlagents_envs.side_channel.engine_configuration_channel import EngineConfigurationChannel
from mlagents_envs.side_channel.environment_parameters_channel import EnvironmentParametersChannel

# 파라미터 값 세팅 
state_size = 8  #에이전트의 입력으로 사용할 고정 벡터 상태의 크기
power_chan_size = 7 #맵에 생성할 수 있는 파워의 총 개수
power_feat_size = 2 #power 1개당 정보크기
sword_chan_size = 2 #맵에 생성할 수 있는 검기의 총 개수 
sword_feat_size = 4 #검기 1개당 정보크기
action_size = 7 #에이전트의 출력으로 사용할 행동의 크기

#환경으로부터 얻는 Observation 벡터에서 각 정보가 담긴 인덱스
POWER_OBS = 0
SWORD_OBS = 1
STATE_OBS = 2

# attention parameter
embed_size = 32
num_heads = 4

load_model = False
train_mode = True

discount_factor = 0.99
learning_rate = 3e-4
n_step = 128
batch_size = 32
n_epoch = 3
_lambda = 0.95
epsilon = 0.2

run_step = 50000000 if train_mode else 0
test_step = 50000

print_interval = 10
save_interval = 100

# 유니티 환경 경로 
game = "AI Duel Game"
os_name = platform.system()
if os_name == 'Windows':
    env_name = f"../Duel_APPO_FSM_Game/{game}"
    
# 모델 저장 및 불러오기 경로
date_time = datetime.datetime.now().strftime("%Y%m%d%H%M%S")
save_path = f"./saved_models/{game}/APPO/{date_time}"
load_path = f"./saved_models/{game}/APPO/20241116121855"

# 연산 장치
device = torch.device("cuda" if torch.cuda.is_available() else "cpu")

# AttentionActorCritic 클래스 -> Attention을 사용하는 ActorCritic Network 정의 
class AttentionActorCritic(torch.nn.Module):
    def __init__(self, **kwargs):
        super(AttentionActorCritic, self).__init__(**kwargs)
        self.attn_power_in = torch.nn.Linear(power_feat_size, embed_size)
        self.attn_power_layer = torch.nn.TransformerEncoderLayer(
            d_model=embed_size, nhead=num_heads, batch_first=True,
            dim_feedforward=embed_size, dropout=0)
        self.attn_power_out = torch.nn.Linear(power_chan_size * embed_size, 128)
        
        self.attn_sword_in = torch.nn.Linear(sword_feat_size, embed_size)
        self.attn_sword_layer = torch.nn.TransformerEncoderLayer(
            d_model=embed_size, nhead=num_heads, batch_first=True,
            dim_feedforward=embed_size, dropout=0)
        self.attn_sword_out = torch.nn.Linear(sword_chan_size * embed_size, 128)
        
        self.e = torch.nn.Linear(state_size, 128)
        self.d1 = torch.nn.Linear(384, 128)
        self.d2 = torch.nn.Linear(128, 128)
        self.pi = torch.nn.Linear(128, action_size)
        self.v = torch.nn.Linear(128, 1)
        
    def forward(self, state):
        power, sword, p_state = torch.split(state, [power_chan_size * power_feat_size, sword_chan_size * sword_feat_size, state_size], dim=1)

        b_p = power.shape[0]
        power = power.reshape(b_p * power_chan_size, power_feat_size)
        attn_power_in = self.attn_power_in(power).reshape(b_p, power_chan_size, embed_size)
        attn_power_out = self.attn_power_layer(attn_power_in)

        power_embed = F.relu(self.attn_power_out(attn_power_out.reshape(b_p, -1)))
        
        b_s = sword.shape[0]
        sword = sword.reshape(b_s * sword_chan_size, sword_feat_size)
        attn_sword_in = self.attn_sword_in(sword).reshape(b_s, sword_chan_size, embed_size)
        attn_sword_out = self.attn_sword_layer(attn_sword_in)

        sword_embed = F.relu(self.attn_sword_out(attn_sword_out.reshape(b_s, -1)))
        
        p_state_embed = F.relu(self.e(p_state))

        x = torch.cat([power_embed, sword_embed, p_state_embed], dim=1)
        x = F.relu(self.d1(x))
        x = F.relu(self.d2(x))
        return F.softmax(self.pi(x), dim=-1), self.v(x)

# AttentionPPOAgent 클래스 -> AttentionPPOAgent 알고리즘을 위한 다양한 함수 정의 
class AttentionPPOAgent:
    def __init__(self, id):
        self.network = AttentionActorCritic().to(device)
        self.optimizer = torch.optim.Adam(self.network.parameters(), lr=learning_rate)
        self.memory = list()
        self.save_path = f"{save_path}/{id}"
        self.load_path = f"{load_path}/{id}"
        self.writer = SummaryWriter(self.save_path)
        
        if load_model == True:
            print(f"... Load Model from {self.load_path}/ckpt ...")
            checkpoint = torch.load(self.load_path+'/ckpt', map_location=device)
            self.network.load_state_dict(checkpoint["network"])
            self.optimizer.load_state_dict(checkpoint["optimizer"])

    # 정책을 통해 행동 결정 
    def get_action(self, state, training=True):
        # 네트워크 모드 설정
        self.network.train(training)

        # 네트워크 연산에 따라 행동 결정
        pi, _ = self.network(torch.FloatTensor(state).to(device))
        action = torch.multinomial(pi, num_samples=1).cpu().numpy()
        return action

    # 리플레이 메모리에 데이터 추가 (상태, 행동, 보상, 다음 상태, 게임 종료 여부)
    def append_sample(self, state, action, reward, next_state, done):
        self.memory.append((state, action, reward, next_state, done))

    # 학습 수행
    def train_model(self):
        self.network.train()

        state      = np.stack([m[0] for m in self.memory], axis=0)
        action     = np.stack([m[1] for m in self.memory], axis=0)
        reward     = np.stack([m[2] for m in self.memory], axis=0)
        next_state = np.stack([m[3] for m in self.memory], axis=0)
        done       = np.stack([m[4] for m in self.memory], axis=0)
        self.memory.clear()

        state, action, reward, next_state, done = map(lambda x: torch.FloatTensor(x).to(device),
                                                        [state, action, reward, next_state, done])
        # prob_old, adv, ret 계산 
        with torch.no_grad():
            pi_old, value = self.network(state)
            prob_old = pi_old.gather(1, action.long())

            _, next_value = self.network(next_state)
            delta = reward + (1 - done) * discount_factor * next_value - value
            adv = delta.clone()
            adv, done = map(lambda x: x.view(n_step, -1).transpose(0,1).contiguous(), [adv, done])
            for t in reversed(range(n_step-1)):
                adv[:, t] += (1 - done[:, t]) * discount_factor * _lambda * adv[:, t+1]
            adv = adv.transpose(0,1).contiguous().view(-1, 1)
            
            ret = adv + value

        # 학습 이터레이션 시작
        actor_losses, critic_losses = [], []
        idxs = np.arange(len(reward))
        for _ in range(n_epoch):
            np.random.shuffle(idxs)
            for offset in range(0, len(reward), batch_size):
                idx = idxs[offset : offset + batch_size]

                _state, _action, _ret, _adv, _prob_old =\
                    map(lambda x: x[idx], [state, action, ret, adv, prob_old])
                
                pi, value = self.network(_state)
                prob = pi.gather(1, _action.long())

                # 정책신경망 손실함수 계산
                ratio = prob / (_prob_old + 1e-7)
                surr1 = ratio * _adv
                surr2 = torch.clamp(ratio, min=1-epsilon, max=1+epsilon) * _adv
                actor_loss = -torch.min(surr1, surr2).mean()

                # 가치신경망 손실함수 계산
                critic_loss = F.mse_loss(value, _ret).mean()

                total_loss = actor_loss + critic_loss

                self.optimizer.zero_grad()
                total_loss.backward()
                self.optimizer.step()

                actor_losses.append(actor_loss.item())
                critic_losses.append(critic_loss.item())

        return np.mean(actor_losses), np.mean(critic_losses)

    # 네트워크 모델 저장
    def save_model(self):
        print(f"... Save Model to {self.save_path}/ckpt ...")
        torch.save({
            "network" : self.network.state_dict(),
            "optimizer" : self.optimizer.state_dict(),
        }, self.save_path+'/ckpt')

    # 학습 기록 
    def write_summary(self, score, actor_loss, critic_loss, step):
        self.writer.add_scalar("run/score", score, step)
        self.writer.add_scalar("model/actor_loss", actor_loss, step)
        self.writer.add_scalar("model/critic_loss", critic_loss, step)
        

# Main 함수 -> 전체적으로 Adversarial PPO 알고리즘을 진행 
if __name__ == '__main__':
    # 유니티 환경 경로 설정 (file_name)
    engine_configuration_channel = EngineConfigurationChannel()
    environment_parameters_channel = EnvironmentParametersChannel()
    env = UnityEnvironment(file_name=env_name,
                           side_channels=[engine_configuration_channel,
                                          environment_parameters_channel])
    env.reset()

    # 유니티 behavior 설정 
    behavior_name_list = list(env.behavior_specs.keys())
    behavior_A = behavior_name_list[0]
    engine_configuration_channel.set_configuration_parameters(time_scale=12.0)
    dec_A, term_A = env.get_steps(behavior_A)

    # PPOAgent 클래스를 agent_A, agent_B로 정의 
    agent_A = AttentionPPOAgent("A")
    
    episode = 0
    actor_losses_A, critic_losses_A, scores_A, score_A = [], [], [], 0
    for step in range(run_step + test_step):
        if step == run_step:
            if train_mode:
                agent_A.save_model()
            print("TEST START")
            train_mode = False
            engine_configuration_channel.set_configuration_parameters(time_scale=1.0)

        preprocess = lambda power, sword, p_state: np.concatenate((power.reshape(-1, power_chan_size*power_feat_size) ,np.concatenate((sword.reshape(-1, sword_chan_size * sword_feat_size), p_state), axis=1)), axis=1)
        state_A = preprocess(dec_A.obs[POWER_OBS], dec_A.obs[SWORD_OBS], dec_A.obs[STATE_OBS])
        action_A = agent_A.get_action(state_A, train_mode)
        action_tuple_A = ActionTuple()
        action_tuple_A.add_discrete(action_A)
        env.set_actions(behavior_A, action_tuple_A)
        env.step()

        dec_A, term_A = env.get_steps(behavior_A)
        done_A = len(term_A.agent_id) > 0
        next_state_A = preprocess(term_A.obs[POWER_OBS], term_A.obs[SWORD_OBS], term_A.obs[STATE_OBS]) if done_A else preprocess(dec_A.obs[POWER_OBS], dec_A.obs[SWORD_OBS], dec_A.obs[STATE_OBS])
        reward_A = term_A.reward if done_A else dec_A.reward
        score_A += reward_A[0]

        if train_mode:
            agent_A.append_sample(state_A[0], action_A[0], reward_A, next_state_A[0], [done_A])
    
            if (step+1) % n_step == 0:
                # 학습 수행
                actor_loss_A, critic_loss_A = agent_A.train_model()
                actor_losses_A.append(actor_loss_A)
                critic_losses_A.append(critic_loss_A)
                
        if done_A:
            episode +=1
            scores_A.append(score_A)
            score_A = 0

            # 게임 진행 상황 출력 및 텐서 보드에 보상과 손실함수 값 기록 
            if episode % print_interval == 0:
                mean_score_A = np.mean(scores_A)
                mean_actor_loss_A = np.mean(actor_losses_A) if len(actor_losses_A) > 0 else 0
                mean_critic_loss_A = np.mean(critic_losses_A) if len(critic_losses_A) > 0 else 0
                agent_A.write_summary(mean_score_A, mean_actor_loss_A, mean_critic_loss_A, step)
                actor_losses_A, critic_losses_A, scores_A = [], [], []

                print(f"{episode} Episode / Step: {step} / "  +\
                      f"A Score: {mean_score_A:.2f} / " +\
                      f"A Actor Loss: {mean_actor_loss_A:.4f} / A Critic Loss: {mean_critic_loss_A:.4f} / ")

            # 네트워크 모델 저장 
            if train_mode and episode % save_interval == 0:
                agent_A.save_model()

    env.close()
