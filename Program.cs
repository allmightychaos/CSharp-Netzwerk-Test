using System.Net;
using System.Net.Sockets;
using System.Text;

Console.WriteLine("Wähle (1) für Host, (2) für Client.");

string answer = Console.ReadLine();

if (answer == "1") { StarteHost(); }
else if (answer == "2") { StarteClient(); }
else { Console.WriteLine("Ungültige Eingabe."); }



// -- Funktionen -- \\

void StarteHost()
{
    Console.WriteLine("Server wird gestartet...");


    // 1. Der "Türsteher"
    // IPAddress.Any -> hört auf alle Netzwerk-Eingänge // 12345 -> Port
    TcpListener listener = new TcpListener(IPAddress.Any, 12345);

    // 2. Türsteher -> "mach die Tür auf"
    listener.Start();
    Console.WriteLine("Server läuft - warte auf Client.");

    // 3. AcceptTcpClient() -> wartet bis Client kommt.
    TcpClient client = listener.AcceptTcpClient();

    Console.Write(client + " - Client verbunden.");
    listener.Stop();
    // ---------------------------------------------------------------------

    // 1. Stream erstellen
    NetworkStream stream = client.GetStream();

    // 2. Leerer "Eimer" (Buffer) mit 1024 Bytes
    byte[] eimer = new byte[1024];

    Console.WriteLine("Warte auf Nachricht vom Client...");

    // 3. Warten auf Nachricht
    int anzahlBytes = stream.Read(eimer, 0, eimer.Length);

    // 4. Bytes -> Text
    string empfangeneNachricht = Encoding.UTF8.GetString(eimer);

    Console.WriteLine($"Eingehende Nachricht: {empfangeneNachricht}");

    // ---------------------------------------------------------------------
    // Fenster nicht sofort beenden
    Console.WriteLine("Drücke ENTER zum Beenden.");
    Console.ReadLine();
}

void StarteClient()
{
    Console.WriteLine("Client wird gestartet...");

    try
    {
        // 1. Client starten
        TcpClient client = new TcpClient();
        // 2. Versuchen den Server zu erreichen, per localhost (127.0.0.1) auf Port 12345
        client.Connect("127.0.0.1", 12345);

        Console.WriteLine("Erfolgreich mit dem Server verbunden");
        // ---------------------------------------------------------------------

        // 1. NetworkStream erstellen
        NetworkStream stream = client.GetStream();

        string nachricht = "SCHUSS|45";

        // 2. Nachricht -> Bytes
        byte[] daten = Encoding.UTF8.GetBytes(nachricht);

        // 3. Paket schicken
        stream.Write(daten, 0, daten.Length);
        Console.WriteLine($"Nachricht: \"{nachricht}\" gesendet.");

        // ---------------------------------------------------------------------
        // Fenster nicht sofort beenden
        Console.WriteLine("Drücke ENTER zum Beenden.");
        Console.ReadLine();
    }
    catch (Exception e) 
    { 
        Console.WriteLine(e.ToString());
        Console.ReadLine();
    }

}