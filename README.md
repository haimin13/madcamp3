# madcamp3
# 📌 프로젝트 개요

> **프로젝트 이름**
> 
> 
> 더 지니어스 게임의법칙 룰브레이커 블랙가넷 그랜드파이널 ???
> 

> **한 줄 소개**
> 
> 
> 장동민 팬 두 명이 만든 더 지니어스 팬게임 
> 

---

# 😉 팀 소개

| 팀원 | github | 역할 |
| --- | --- | --- |
| 임하민 | https://github.com/haimin13 | 프론트엔드, 유니티 내 디자인(총괄.) |
| 윤신이 | https://github.com/tlsdl6942 | 백엔드, 유니티 외 디자인 |

# 🛠 주요 기능

## 더 지니어스 시리즈에 나오는 ~~다양한~~ 게임들을 즐길 수 있어요!!

---

# 🎥 앱 시연

> 📷 대표 스크린샷
> 

![image.png](attachment:51057f35-4cc4-431b-8647-05088cf91fd7:image.png)

![image.png](attachment:6f087e42-5775-4336-9cfa-9f5da0e6d060:image.png)

![image.png](attachment:2ddcff19-4555-4510-9eed-79eabcece92e:image.png)

![image.png](attachment:f6eb4974-db06-4c47-8506-7e211d9a5da3:image.png)

---

# 💻 개발 과정

## 🔍 주요 기술스택

> \(ㅇㅂㅇ)/
> 
> 
> 
> | 분야 | 기술 |
> | --- | --- |
> | 🎮 프론트엔드 | Unity (C#) |
> | 🧠 백엔드 | Python + Flask (REST API) |
> | 🎨 디자인 | Aesprite + Figma |
> | 📡 통신 | HTTP (REST API) |
> | 🖥️ 서버 | AWS EC2 (Ubuntu) |

---

📝 REST API 프로토콜
| 기능            | URL                | Method | 요청 필드                                    | 응답 필드                                                  | 설명                |
| ------------- | ------------------ | ------ | ---------------------------------------- | ------------------------------------------------------ | ----------------- |
| 로그인           | `/login`           | POST   | userName                                 | result, user\_id, user\_name                           | 사용자 로그인           |
| 말 이동 가능 위치 요청 | `/available-moves` | POST   | session\_id, player\_id, piece, position | result, moves                                          | 이동 가능한 좌표 요청      |
| 말 이동          | `/move`            | POST   | session\_id, player\_id, piece, position | result, capture, is\_end, winner                       | 실제 말 이동 처리        |
| 드롭 위치 요청      | `/available-drop`  | POST   | session\_id, player\_id, piece           | result, moves                                          | 잡은 말 드롭 가능한 위치 요청 |
| 말 드롭          | `/drop`            | POST   | session\_id, player\_id, piece, position | result, is\_end, winner                                | 말 드롭 처리           |
| 방 생성          | `/create-room`     | POST   | user\_id, game, roomName                 | result, session\_id, player\_id, roomName, roomPW      | 게임 방 생성           |
| 방 입장          | `/enter-room`      | POST   | user\_id, roomName, roomPW               | result, session\_id, player\_id                        | 방 입장 처리           |
| 게임 시작 대기      | `/ready`           | POST   | session\_id, player\_id                  | result, startSignal                                    | 게임 시작 여부 대기       |
| 내 턴인지 확인      | `/wait-turn`       | POST   | session\_id, player\_id                  | result, turn, op\_position, op\_piece, is\_end, winner | 턴 확인 및 상대 행동 전달   |
| 타임아웃 처리       | `/timeout`         | POST   | session\_id, player\_id                  | result, is\_end                                        | 시간 초과 시 처리        |

---

📱 .EXE 다운로드
