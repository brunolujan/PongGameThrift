using System;
using System.Threading;
using System.Threading.Tasks;
using Thrift.Server;
using Thrift.Transport;
using Thrift.Transport.Server;

namespace PongServer {

    class Server {

        static void Main(string[] args) {

            var serverPong = new PongImplemetation();
            var processor = new Pong.AsyncProcessor(serverPong);
            TServerTransport transport = new TServerSocketTransport(5000);
            TServer server = new TThreadPoolAsyncServer(processor, transport);
            Console.WriteLine("Pong server has been started. \nPress Enter button to end...");
            server.ServeAsync(new CancellationToken());
            Task.Run(() => {

                while (true) {
                    if (serverPong.players.Count == 2) {
                        if (!serverPong.MoveBall())
                        {
                            Thread.Sleep(1000);
                            serverPong.ClearGame();
                            Console.WriteLine("Game Over. \nPlease, run the players again to play again");
                        }
                        Thread.Sleep(70);
                    }
                }

            });

            while (true) {
                if (Console.Read() >= 0) {
                    break;
                }
            }

            server.Stop();

        }
    }
}