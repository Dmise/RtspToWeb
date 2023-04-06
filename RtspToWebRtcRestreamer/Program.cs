using RtspToWebRtcRestreamer;
using static Org.BouncyCastle.Math.EC.ECCurve;

var _wsPort = 5300;

//Create and Run Demuxer
var demuxer = new FFmpegDemuxer();
demuxer.Run();

// Create and run Listener
var ffmpegListener = new FFmpegListener(demuxer.GetConfig());
var listenerToken = new CancellationToken();
Task.Run(() => ffmpegListener.Run(listenerToken));
while (!ffmpegListener.ready) { }

//Create and Run WebSocketServer
var wsServer = new WebSocketSignalingServer(ffmpegListener, _wsPort);
wsServer.Run();


// Waiting connection from webbrowserW
// exit loop
var running = true;
var readTask = new Task(() =>
{
    while (true)
    {
        var input = Console.ReadKey(true);
        if (input.KeyChar == 'q')
        {
            running = false;
            break;
        }
    }
});
readTask.Start();

while (running)
{

}
