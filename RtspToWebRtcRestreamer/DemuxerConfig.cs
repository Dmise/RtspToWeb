using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RtspToWebRtcRestreamer
{
    internal class DemuxerConfig
    {
        // exe path
        public string SdpFolder { get; private set; } = "A:\\temp\\sdp";
        public string SdpFileName { get; private set; } = "stream.sdp";
        public string FfmpegBinaryFolder { get; private set; } = "C:\\Program Files\\ffmpeg\\bin";
        

        // ffmpeg command settings
        public string commandTemplate = "-re -i {0} -an -vcodec {1} -ssrc {2} -f rtp rtp://{3}:{4} -vn -acodec {5} -ssrc {6} -f rtp rtp://{3}:{7} -sdp_file {8}";
        public string rtspUrl = "rtsp://admin:HelloWorld4@192.168.1.64:554/ISAPI/Streaming/Channels/101";
        public string vcodec = "h264";
        public string acodec = "pcm_alaw";
        public int audioPort = 5204;
        public int videoPort = 5202;
        public uint audioSsrc = 50;
        public uint videoSsrc = 60;
        public string serverIP = "127.0.0.1";
        public string sdpPath  
        {
            get { return Path.Combine(SdpFolder, SdpFileName); } 
        }
    }
}
