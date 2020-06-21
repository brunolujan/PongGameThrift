using System;
using Thrift;
using Thrift.Protocol;
using Thrift.Transport;
using System.Threading;
using System.Threading.Tasks;
using Thrift.Transport.Client;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Http.Features;
using System.Diagnostics;
using System.Threading.Channels;

namespace PongClient {

    class Client {

        private static bool isPlaying = false;
        private static bool isGameOver = false;
        private static bool isGameStarted = false;
        private static int scorePlayerOne = 0;
        private static int scorePlayerTwo = 0;
        private static string scoreboard = "";
        private static int idOwner = 0;
        private static int idOpponent = 0;
        private static Pong.Client client;
        private static List<Player> players = new List<Player>();
        private const int racketSize = 6;
        private const int boardWidth = 70;
        private const int boardHeight = 20;
        private const int scoreToWin = 5;
        private static Position ballPosition;

        static async Task Main(string[] args) {

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                Console.SetWindowSize(boardWidth, boardHeight);
                Console.BufferWidth = boardWidth;
                Console.BufferHeight = boardHeight;
            }

            ballPosition = new Position { X = boardWidth / 2, Y = boardHeight / 2 };

            players.Add(new Player {
                IdPlayer = 0,
                Position = new Position { X = 0, Y = (boardHeight / 2 - racketSize / 2)}
            });

            players.Add(new Player {
                IdPlayer = 1,
                Position = new Position { X = boardWidth - 1, Y = (boardHeight / 2 - racketSize / 2)}
            });

            try {

                TTransport transport = new TSocketTransport("localhost", 5000);
                TProtocol protocol = new TBinaryProtocol(transport);
                client = new Pong.Client(protocol);

                while (true) {
                    if (!isPlaying) {
                        idOwner = await client.JoinGameAsync();
                        idOpponent = idOwner % 2 == 0 ? idOwner + 1 : idOwner - 1;
                        await client.SendPositionAsync(players[idOwner]);
                        isPlaying = true;
                    }

                    if (!Console.KeyAvailable) {
                        try {
                            players[idOpponent] = await client.GetPlayerPositionAsync(idOpponent);
                            isGameStarted = true;
                        } catch (PlayerNotFound) {
                            if (isGameStarted) {
                                isGameStarted = false;
                            } else {
                                scoreboard = "Hey crack we're wating for an opponent ;)";
                            }
                        }

                        InitializeBoard();

                        if (isGameOver) {
                            break;
                        }

                        if (isGameStarted) {
                            scoreboard = scorePlayerOne + " - " + scorePlayerTwo;
                            InitializeBoard();
                            Console.SetCursorPosition(ballPosition.X, ballPosition.Y);
                            Console.Write("*");
                            MoveBall();
                        }

                        Thread.Sleep(70);
                        continue;

                    }
                    switch (Console.ReadKey().Key) {
                        case ConsoleKey.UpArrow:
                            if (players[idOwner].Position.Y > 0) {
                                players[idOwner].Position.Y--;
                                await client.SendPositionAsync(players[idOwner]);
                            }
                            break;

                        case ConsoleKey.DownArrow:
                            if (players[idOwner].Position.Y < (boardHeight - racketSize)) {
                                players[idOwner].Position.Y++;
                                await client.SendPositionAsync(players[idOwner]);
                            }
                            break;
                    }
                }

                transport.Close();
                Console.Read();

            } catch (TApplicationException tApplicationE) {
                Console.Clear();
                Console.WriteLine(tApplicationE.StackTrace);
                Console.Read();
            } catch (Exception) {
                Console.Clear();
                Console.WriteLine("Error trying to connect to server, F.");
                Console.Read();
            }
        }

        private static void InitializeBoard() {

            Console.Clear();

            for (int i = 0; i < racketSize; i++) {
                Console.SetCursorPosition(players[0].Position.X, players[0].Position.Y + i);
                Console.Write("|");
                Console.SetCursorPosition(players[1].Position.X, players[1].Position.Y + i);
                Console.Write("|");
            }

            Console.SetCursorPosition((boardWidth / 2) - (scoreboard.Length / 2), 0);
            Console.Write(scoreboard);

        }

        private static async void MoveBall() {

            ballPosition = await client.GetBallPositionAsync();
            if (ballPosition.X == (boardWidth / 2) - 1 &&
                ballPosition.Y == (boardHeight / 2) - 1) {

                await ShowScoreBoard();

            }
        }

        private static async Task ShowScoreBoard() {

            int scorePlayer1 = await client.GetScoreAsync(idOwner);
            int scorePlayer2 = await client.GetScoreAsync(idOpponent);

            if (idOwner % 2 == 0) {
                scorePlayerOne = scorePlayer1;
                scorePlayerTwo = scorePlayer2;
            } else {
                scorePlayerOne = scorePlayer2;
                scorePlayerTwo = scorePlayer1;
            }

            if (scorePlayer1 == scoreToWin) {
                scoreboard = "CONGRATULATIONS! YOU'RE OUR WINNER";
                isGameOver = true;
            } else if (scorePlayer2 == scoreToWin) {
                scoreboard = "F TO YOU :( TRY AGAIN";
                isGameOver = true;
            }
        }

    }
}
