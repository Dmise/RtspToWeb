﻿using SIPSorcery.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RtspToWebRtcRestreamer
{
    /// <summary>
    /// Create FFmpeg process that split rtsp stream into two RTP stream (audio+video)
    /// </summary>
    internal class FFmpegDemuxer
    {     
        private Process _ffmpegProcess;
        private DemuxerConfig _dc;        

        public DemuxerConfig GetConfig()
        {
            return _dc;
        }
        public FFmpegDemuxer()
        {
            
            _dc = new DemuxerConfig
            {
                commandTemplate = "-re -i {0} -an -vcodec {1} -ssrc {2} -f rtp rtp://{3}:{4} -vn -acodec {5} -ssrc {6} -f rtp rtp://{3}:{7} -sdp_file {8}",
                rtspUrl = "rtsp://admin:HelloWorld4@192.168.1.64:554/ISAPI/Streaming/Channels/101",
                vcodec = "h264",
                acodec = "copy",
                audioPort = 5204,
                videoPort = 5202,
                audioSsrc = 50,
                videoSsrc = 60,
                serverIP = IPAddress.Loopback.MapToIPv4().ToString(),
            };
            
    }

        public void Run()
        {
            var args = String.Format(_dc.commandTemplate, 
                _dc.rtspUrl,
                _dc.vcodec,
                _dc.videoSsrc,
                _dc.serverIP,
                _dc.videoPort,
                _dc.acodec,
                _dc.audioSsrc,
                _dc.audioPort,
                _dc.sdpPath
                );
            SetupAndRunProcess(ref _ffmpegProcess, args);
        }


        void SetupAndRunProcess(ref Process proc, string arguments)
        {
            proc = new Process();
            proc.StartInfo.FileName = Path.Combine(_dc.FfmpegBinaryFolder, "ffmpeg");
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.OutputDataReceived += FFMpegOutputLog;
            proc.ErrorDataReceived += FFMpegOutputError;
            proc.StartInfo.Arguments = arguments;
            proc.StartInfo.WorkingDirectory = _dc.FfmpegBinaryFolder;
                   
            if (!Directory.Exists(_dc.SdpFolder)) Directory.CreateDirectory(_dc.SdpFolder);
            proc.Start();
            proc.BeginErrorReadLine();
            proc.BeginOutputReadLine();
        }

        private void FFMpegOutputError(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        private void FFMpegOutputLog(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }
    }
}