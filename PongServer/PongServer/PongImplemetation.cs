using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Reflection.Metadata.Ecma335;
using System.IO.Pipes;

namespace PongServer {

    class PongImplemetation : Pong.IAsync {

        public List<Player> players = new List<Player>();
        private int scorePlayerOne = 0;
        private int scorePlayerTwo = 0;
        private const int racketSize = 6;
        private const int boardWidth = 70;
        private const int boardHeight = 20;
        private const int scoreToWin = 5;
        private static bool isBallGoingUp = true;
        private static bool isBallGoingLeft = true;
        private static Position ballPosition = new Position {

            X = boardWidth / 2,
            Y = boardHeight / 2

        };

        public bool MoveBall() {

            if (scorePlayerOne == scoreToWin || scorePlayerTwo == scoreToWin) {
                return false;
            }

            if (ballPosition.Y == 0) {
                isBallGoingUp = false;
            } else if (ballPosition.Y == boardHeight - 1) {
                isBallGoingUp = true;
            }

            if (ballPosition.X == 0) {
                scorePlayerTwo++;
                ResetBall();
            } else if (ballPosition.X == boardWidth - 1) {
                scorePlayerOne++;
                ResetBall();
            }

            if (ballPosition.X == players[0].Position.X + 1 &&
                (ballPosition.Y >= players[0].Position.Y &&
                ballPosition.Y <= players[0].Position.Y + (racketSize - 1))) {
                isBallGoingLeft = false;
            } else if (ballPosition.X == players[1].Position.X - 1 &&
                (ballPosition.Y >= players[1].Position.Y && 
                ballPosition.Y <= players[1].Position.Y + (racketSize - 1))) {
                isBallGoingLeft = true;
            }

            if (isBallGoingLeft) {
                ballPosition.X--;
            } else {
                ballPosition.X++;
            }

            if (isBallGoingUp) {
                ballPosition.Y--;
            } else {
                ballPosition.Y++;
            }

            return true;

        }

        private void ResetBall() {

            ballPosition.X = boardWidth / 2;
            ballPosition.Y = boardHeight / 2;
            isBallGoingUp = true;
            isBallGoingLeft = true;

        }

        public void ClearGame() {

            players.Clear();
            scorePlayerOne = 0;
            scorePlayerTwo = 0;

        }

        public Task<int> JoinGameAsync(CancellationToken cancellationToken = default) {
            var idNewPlayer = players.Count;
            players.Add(new Player {

                IdPlayer = idNewPlayer,

                Position = new Position {

                    X = 0,
                    Y = 0

                }
            });

            Console.WriteLine("Player " + players.Count + " is  ready to kick your ass");
            return Task.FromResult(idNewPlayer);
            
        }

        public Task<Position> GetBallPositionAsync(CancellationToken cancellationToken = default) {
            return Task.FromResult(ballPosition);
        }

        public Task<int> GetScoreAsync(int idPlayer, CancellationToken cancellationToken = default) {
            if (players.Find(j => j.IdPlayer == idPlayer) == null) {
                throw new PlayerNotFound {
                    IdPlayer = idPlayer,
                    Message = "Sorry bro, I can't find any player with that id"
                };
            }

            if (idPlayer == 0) {
                return Task.FromResult(scorePlayerOne);
            } else {
                return Task.FromResult(scorePlayerTwo);
            }
        }

        public Task SendPositionAsync(Player player, CancellationToken cancellationToken = default) {
            players[player.IdPlayer] = player;
            return Task.CompletedTask;
        }

        public Task<Player> GetPlayerPositionAsync(int idPlayer, CancellationToken cancellationToken = default) {
            if (players.Find(j => j.IdPlayer == idPlayer) == null) {
                throw new PlayerNotFound {
                    IdPlayer = idPlayer,
                    Message = "Sorry bro, I can't find any player with that id"
                };
            }
            return Task.FromResult(players[idPlayer]);
        }
    }
}
