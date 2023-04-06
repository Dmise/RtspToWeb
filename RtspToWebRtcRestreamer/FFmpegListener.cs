using Org.BouncyCastle.Security;
using SIPSorcery.Net;
using SIPSorcery.SIP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RtspToWebRtcRestreamer
{   
    /// <summary>
    /// Listne RTP streams created by ffmpeg process
    /// </summary>
    internal class FFmpegListener
    {
        private RTPSession _videoRTP;
        private RTPSession _audioRTP;      
        private DemuxerConfig _dc;
        public bool ready = false;
        public MediaStreamTrack? videoTrack { get; private set; }
        public MediaStreamTrack? audioTrack { get; private set; }
        public SDPAudioVideoMediaFormat videoFormatRTP { get; private set; }
        public SDPAudioVideoMediaFormat audioFormatRTP { get; private set; }

        public event Action<IPEndPoint, SDPMediaTypesEnum, RTPPacket> OnAudioRtpPacketReceived;
        public event Action<IPEndPoint, SDPMediaTypesEnum, RTPPacket> OnVideoRtpPacketReceived;

        public FFmpegListener( DemuxerConfig demuxConfig)
        {         
            _dc = demuxConfig;
        }       

        public async void Run(CancellationToken token)
        {
            // read sdp file
            var sdpVideo = SDP.ParseSDPDescription(File.ReadAllText(_dc.sdpPath));
            var audioAnn = sdpVideo.Media.Find(x => x.Media == SDPMediaTypesEnum.audio);
            sdpVideo.Media.Remove(audioAnn);

            // configure video listener
            var videoAnn = sdpVideo.Media.Find(x => x.Media == SDPMediaTypesEnum.video);
            videoFormatRTP = videoAnn.MediaFormats.Values.First();
            videoTrack = new MediaStreamTrack(
                                        SDPMediaTypesEnum.video,
                                        false,
                                        new List<SDPAudioVideoMediaFormat> { videoFormatRTP },
                                        MediaStreamStatusEnum.RecvOnly);
            videoTrack.Ssrc = _dc.videoSsrc;
            _videoRTP = new RTPSession(false, false, false, IPAddress.Loopback, _dc.videoPort);
            _videoRTP.AcceptRtpFromAny = true;
            _videoRTP.SetRemoteDescription(SIPSorcery.SIP.App.SdpType.answer, sdpVideo);
            _videoRTP.addTrack(videoTrack);

            // configure audio listener
            var sdpAudio = SDP.ParseSDPDescription(File.ReadAllText(_dc.sdpPath));
            sdpAudio.Media.Remove(videoAnn);
            
            audioFormatRTP = audioAnn.MediaFormats.Values.First();
            audioTrack = new MediaStreamTrack(
                                        SDPMediaTypesEnum.audio,
                                        false,
                                        new List<SDPAudioVideoMediaFormat> { audioFormatRTP },
                                        MediaStreamStatusEnum.RecvOnly);
            audioTrack.Ssrc = _dc.audioSsrc;
            _audioRTP = new RTPSession(false, false, false, IPAddress.Loopback, _dc.audioPort);
            _audioRTP.AcceptRtpFromAny = true;
            _audioRTP.SetRemoteDescription(SIPSorcery.SIP.App.SdpType.answer, sdpAudio);
            _audioRTP.addTrack(audioTrack);

            //Start listen
            //var dummyIPEndPoint = new IPEndPoint(IPAddress.Loopback, 0);
            //_videoRTP.SetDestination(SDPMediaTypesEnum.video, dummyIPEndPoint, dummyIPEndPoint);
            //_audioRTP.SetDestination(SDPMediaTypesEnum.audio, dummyIPEndPoint, dummyIPEndPoint);

            _videoRTP.OnRtpPacketReceived += HndlVideoPacketReceived;
            _audioRTP.OnRtpPacketReceived += HndlAudioPacketReceived;
            await _videoRTP.Start();
            await _audioRTP.Start();
            ready = true;
            
            
        }
        private void HndlVideoPacketReceived(IPEndPoint arg1, SDPMediaTypesEnum arg2, RTPPacket arg3)
        {
            if (OnVideoRtpPacketReceived == null) return;
            OnVideoRtpPacketReceived.Invoke(arg1, arg2, arg3);
        }
        private void HndlAudioPacketReceived(IPEndPoint arg1, SDPMediaTypesEnum arg2, RTPPacket arg3)
        {
            if (OnVideoRtpPacketReceived == null) return;
            OnAudioRtpPacketReceived.Invoke(arg1, arg2, arg3);
        }
    }
}
