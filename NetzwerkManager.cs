using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Walkie_Talkie
{
    public class NetzwerkManager
    {
        // --- Host & Client --- \\
        async Task StarteHost()
        {
            Console.WriteLine("Server wird gestartet...");

            // 1. Der "Türsteher"
            // IPAddress.Any -> hört auf alle Netzwerk-Eingänge // 12345 -> Port
            TcpListener listener = new TcpListener(IPAddress.Any, 12345);

            // 2. Türsteher -> "mach die Tür auf"
            listener.Start();
            Console.WriteLine("Server läuft, warten auf Client.");

            // 3. AcceptTcpClient() -> wartet bis Client kommt.
            TcpClient client = await listener.AcceptTcpClientAsync();

            // Console.Write(client + " - Client verbunden.");
            listener.Stop();


            // ---------------------------------------------------------------------


            NetworkStream stream = client.GetStream();

            Console.WriteLine("Warte auf Nachricht vom Client...");


            while (client.Connected)
            {
                // -) Nachricht erhalten
                if (!await getMessage(stream)) { break; }
            }


            // -) Verbindung schließen
            stream.Close();
            client.Close();


            // ---------------------------------------------------------------------
            // -) Fenster nicht sofort beenden
            Console.WriteLine("Drücke ENTER zum Beenden.");
            Console.ReadLine();
        }

        async Task StarteClient()
        {
            Console.WriteLine("Client wird gestartet...");

            try
            {
                // -) Client starten
                TcpClient client = new TcpClient();
                await client.ConnectAsync("127.0.0.1", 12345); // localhost -- port 12345

                Console.WriteLine("Erfolgreich mit dem Server verbunden");


                // ---------------------------------------------------------------------


                NetworkStream stream = client.GetStream();

                // -) Nachricht senden
                sendMessage(stream, null);

                while (client.Connected)
                {
                    // -) Nachricht erhalten
                    if (!await getMessage(stream)) { break; }
                }


                stream.Close();
                client.Close();


                // ---------------------------------------------------------------------
                // -) Fenster nicht sofort beenden
                Console.WriteLine("Drücke ENTER zum Beenden.");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.ReadLine();
            }

        }


        // --- Hilfsfunktionen --- \\

        void sendMessage(NetworkStream stream, string? message)
        {
            string answer;

            if (message == null)
            {
                Console.WriteLine("\n\nGib deine Nachricht ein (oder \"q\" um die Verbindung zu schließen):");
                answer = Console.ReadLine();
            }
            else
            {
                answer = message;
            }

            // -) Nachricht -> Bytes
            byte[] daten = Encoding.UTF8.GetBytes(answer);

            // -) Paket schicken
            stream.Write(daten, 0, daten.Length);
            Console.WriteLine($"Nachricht: \"{answer}\" gesendet.");
        }

        async Task<bool> getMessage(NetworkStream stream)
        {
            byte[] eimer = new byte[1024];

            // -) Warten auf Nachricht
            int anzahlBytes = await stream.ReadAsync(eimer, 0, eimer.Length);
            if (anzahlBytes == 0) { return false; } // wenn Bytes = 0 -> Verbindung wurde geschlossen
                                                    // Console.WriteLine("Warten auf Nachricht...");

            // -) Bytes -> Text
            string empfangeneNachricht = Encoding.UTF8.GetString(eimer, 0, anzahlBytes);
            Console.WriteLine($"Eingehende Nachricht: {empfangeneNachricht}. Wird verarbeitet.");

            // -) Nachricht verarbeiten
            HandleMessage(stream, empfangeneNachricht);

            return true;
        }

        void HandleMessage(NetworkStream stream, string message)
        {
            string[] parts = message.Split('|');
            string command = parts[0];
            int args = 0;

            if (parts.Length > 1)
            {
                args = int.Parse(parts[^1]);
            }
            //Console.WriteLine($"Command: {command} - args: {args}.");

            switch (command)
            {
                case "SCHUSS":
                    // Console.WriteLine("case SCHUSS");
                    sendMessage(stream, checkHit(args));
                    break;
                case "WASSER":
                case "TREFFER":
                case "VERSENKT":
                    // Console.WriteLine("case WASSER/TREFFER/VERSENKT");
                    sendMessage(stream, "OK");
                    break;
                case "OK":
                    // Console.WriteLine("case OK");
                    sendMessage(stream, null);
                    break;
                case "UNGÜLTIG":
                    sendMessage(stream, null);
                    break;

                default:
                    sendMessage(stream, "UNGÜLTIG");
                    break;
            }
        }

        string checkHit(int index)
        {
            return "WASSER";
        }
    }
}
