using System.Device.Gpio;
using System.Device.Spi;
using Iot.Device.Mfrc522;
using Iot.Device.Rfid;

string GetCardId(Data106kbpsTypeA card) => Convert.ToHexString(card.NfcId);

GpioController gpioController = new GpioController();
int pinReset = 21;

SpiConnectionSettings connection = new(0, 0);
connection.ClockFrequency = 10_000_000;


var source = new CancellationTokenSource();
var token = source.Token;

var task = Task.Run(() => ReadData(token), token);

Console.WriteLine("Any key to close.");
Console.ReadKey();

source.Cancel();

await task;

void ReadData(CancellationToken cancellationToken)
{
    Console.WriteLine("Task run.");
    var active = true;

    do
    {
        if (cancellationToken.IsCancellationRequested)
        {
            active = false;
        }

        try
        {
            using (SpiDevice spi = SpiDevice.Create(connection))
            using (MfRc522 mfrc522 = new(spi, pinReset, gpioController, false))
            {

                Data106kbpsTypeA card;
                var res = mfrc522.ListenToCardIso14443TypeA(out card, TimeSpan.FromSeconds(2));

                if (res)
                {
                    var cardId = GetCardId(card);
                    Console.WriteLine(cardId);
                }
            }

            Thread.Sleep(1000);
        }
        catch (System.Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    } while (active);

    Console.WriteLine("Task done.");
}