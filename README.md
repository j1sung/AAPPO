# 🎮 AAPPO

**대전 상대 유형이 플레이어 경험에 미치는 영향: Human, FSM, 강화 학습 AI의 비교 연구**  
_Evaluating the Impact of Opponent Types on Player Experience: A Comparative Study of Human, FSM, and Reinforcement Learning AI in Game Environments_


## 📌 프로젝트 정보

- 🏷️ 제출 학회: HCI KOREA 2025  
- 📄 논문 보기: [DBpia 바로가기](https://www.dbpia.co.kr/journal/articleDetail?nodeId=NODE12131720)  
- 🗓️ 프로젝트 기간: 2024.10~2024.12


## 🧪 연구 개요

본 연구는 **게임 내 대전 상대 유형(Human, FSM, PPO 기반 AI)** 이  
**플레이어 경험(몰입, 유능감, 조작 직관성)에 미치는 영향**을 분석하기 위해 설계되었습니다.

- **장르**: 1:1 대전 액션 게임  
- **승리 조건**: 3회 타격 성공 시 1라운드 승리 / 라운드제 진행  
- **조작 방식**: 키보드 기반 (WASD + Space + Shift)


## 🧠 상대 유형 설계

| 유형 | 설명 | 구현 방식 |
|------|------|-----------|
| 🧑 Human | 실제 사람과의 실시간 대전 | Photon PUN2 기반 |
| ⚙ FSM | 상태 기반 전이 로직 | Finite State Machine |
| 🤖 강화학습 AI | PPO(Proximal Policy Optimization) 알고리즘 사용 | Unity ML-Agents |

FSM 및 AI는 다음과 같은 행동 상태를 기반으로 설계됩니다:
- `Idle`, `Patrolling`, `Chasing`, `Item`, `Attacking`, `Dash`


## 🎮 게임 조작 방법

| 키 | 동작 |
|----|------|
| W / S | 바라보는 방향으로 전진 / 후진 |
| A / D | 좌우 회전 |
| Space | 대시 (아이템 사용 시, 무적 판정) |
| Shift | 원거리 공격 (아이템 소모) |


## 🧪 실험 설계 및 결과 요약

- **참가자**: 총 15명 (성인)
- **실험 과정**:
  1. 참가자는 Human, FSM, AI 순으로 게임 플레이
  2. 각 플레이 후 **GEQ**, **PENS** 설문 응답
  3. 수집된 결과를 기반으로 **일원배치 분산분석(ANOVA)** 및 **Tukey HSD** 검정 수행

### 📊 주요 결과

| 비교 항목 | 결과 |
|-----------|------|
| 유능감(Competence) | Human 상대 > AI 상대 (통계적으로 유의미한 차이) |
| 몰입(Flow), 조작 직관성(Intuitive Control) | 세 그룹 간 유의미한 차이 없음 |
| 분석 해석 | Human은 명확한 피드백과 도전적 과제 제공 → 자기 효능감 강화 |

> ✅ **AI와 FSM은 유사한 기술 수준의 상호작용 제공**,  
> 🤖 향후 Human 상대의 강점을 반영한 AI 설계 필요


## 🛠️ 기술 스택

| 도구 | 사용 목적 |
|------|-----------|
| Unity | 게임 개발 |
| Photon Pun 2 | 1:1 멀티플레이 |
| ML-Agents + PPO | 강화학습 AI 학습 및 적용 |
| Google Forms | 설문 수집 및 통계 분석 |


## 👥 저자 및 팀원

| 이름 | 소속 | 이메일 |
|------|------|--------|
| 김하연 | 단국대학교 | qoocrab@gmail.com |
| 임백규 | 단국대학교 | imbq8603@gmail.com |
| 김호영 | 단국대학교 | k4h4y1@gmail.com |
| 안지성 | 단국대학교 | dalssagi00@gmail.com |
| 김영채 | 단국대학교 | acausea01@gmail.com |

> ※ 본 연구는 2024년 과학기술정보통신부 및 정보통신기획평가원의 SW 중심대학 사업 지원을 받아 수행되었습니다. (과제번호: 2024-0-00035)


## 📎 관련 링크

- [Photon PUN 2 - Unity Asset Store](https://assetstore.unity.com/packages/tools/network/pun-2-free-119922)  
- [Unity ML-Agents Toolkit](https://github.com/Unity-Technologies/ml-agents)  
- [HCI KOREA 공식 홈페이지](https://hcikorea.org/)
