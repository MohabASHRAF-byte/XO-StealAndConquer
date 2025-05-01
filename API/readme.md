# XO Modern: A Real-Time, Multiplayer Twist on Tic-Tac-Toe

**XO Modern** is a real-time, multiplayer game that reimagines the classic Tic-Tac-Toe (XO) with innovative rules and strategic depth. Unlike traditional XO, this game introduces cell-stealing mechanics, custom labels, and a judge system, all powered by tech stack featuring SignalR for real-time communication and a robust API for backup functionality.

## Features
- **Real-Time Gameplay**: Powered by SignalR for instant updates and live interactions.
- **Multiplayer Support**: Requires at least three players and a judge per game.
- **Custom Labels**: Each row and column has unique labels, requiring teams to submit answers matching their intersection.
- **Judgment System**: A judge evaluates answers to assign or deny cells.
- **Cell Stealing**: Players can steal cells previously claimed by the opposing team.
- **Win Condition**: The first team to secure three consecutive cells (horizontally, vertically, or diagonally) wins.

## Technologies Used
- **Backend**: ASP.NET Core
- **Real-Time Communication**: SignalR
- **API Documentation**: Swagger
- **Database**: Postgres
- **Authentication**: JWT

## Game Rules
1. **Players and Roles**: Each game requires at least three players and one judge.
2. **Labels and Answers**: Rows and columns have labels. Teams submit answers during their turn that match the intersection of these labels.
3. **Judgment**: The judge reviews the answer and decides if the team claims the cell.
4. **Cell Stealing**: Players can select and steal cells already claimed by the opposing team.
5. **Winning**: The first team to achieve three consecutive cells wins.

## Real-Time and API Support
- **SignalR**: Enables real-time updates for cell selections, steals, and judgments, ensuring a seamless multiplayer experience.
- **API Backup**: All game functions (joining, submitting answers, judging, etc.) are supported by API endpoints, providing reliability and flexibility.

## How to Run the Backend
1. **Clone the Repository**:
   ```bash
   git clone https://github.com/yourusername/xo-modern.git
   ```
2. **Install Dependencies**:
   ```bash
   dotnet restore
   ```
3. **Configure the Database**:
    - [Add database setup instructions, e.g., run migrations]
4. **Run the Application**:
   ```bash
   dotnet run
   ```
5. **Access Swagger**:
    - Visit `http://localhost:0000/swagger` to explore and test the API.
    - change with your port number