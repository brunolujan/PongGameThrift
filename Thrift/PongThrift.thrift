struct Position {
    1: i32 x
    2: i32 y
}
struct Player {
    1: i32 idPlayer
    2: Position position
}

exception PlayerNotFound {
	1: i32 idPlayer
    2: string message
}

service Pong {

	i32 JoinGame()
    
    Position GetBallPosition()
    
    i32 GetScore(1: i32 idPlayer)
    
    void SendPosition(1: Player player)

    Player GetPlayerPosition(1: i32 idPlayer) throws (1: PlayerNotFound playerNotFoundE)

}
