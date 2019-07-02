using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JRJLMonitor
{
    class Camera
    {
        private Int32 m_lUserID = -1;
        private bool m_bInitSDK = false;
        private Int32 m_lRealHandle = -1;
        CHCNetSDK.REALDATACALLBACK RealData = null;
        //
        private string DVRIPAddress = ""; //设备IP地址或者域名
        private Int16 DVRPortNumber = 8000;//设备服务端口号
        private string DVRUserName = "";//设备登录用户名
        private string DVRPassword = "";//设备登录密码

        public Camera(string ip, Int16 port, string username, string password)
        {
            DVRIPAddress = ip;
            DVRPortNumber = port;
            DVRUserName = username;
            DVRPassword = password;
            m_bInitSDK = CHCNetSDK.NET_DVR_Init();
        }
        ~Camera()
        {
            if (m_lRealHandle >= 0) CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandle);
            if (m_lUserID >= 0) CHCNetSDK.NET_DVR_Logout(m_lUserID);
            if (m_bInitSDK == true) CHCNetSDK.NET_DVR_Cleanup();
        }
        public Boolean Login()
        {
            if (m_lUserID >= 0) return false;
            CHCNetSDK.NET_DVR_DEVICEINFO_V30 DeviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V30();
            m_lUserID = CHCNetSDK.NET_DVR_Login_V30(DVRIPAddress, DVRPortNumber, DVRUserName, DVRPassword, ref DeviceInfo); ////登录设备 Login the device
            Console.WriteLine(DVRIPAddress + DVRPortNumber + DVRUserName + DVRPassword);
            if (m_lUserID >= 0) { return true; } else { Console.WriteLine(m_lUserID); return false; }
            
        }
        public void RealDataCallBack(Int32 lRealHandle, UInt32 dwDataType, IntPtr pBuffer, UInt32 dwBufSize, IntPtr pUser)
        {
            if (dwBufSize <= 0) return;
            byte[] sData = new byte[dwBufSize];
            System.Runtime.InteropServices.Marshal.Copy(pBuffer, sData, 0, (Int32)dwBufSize);
            string str = "实时流数据.ps";
            FileStream fs = new FileStream(str, FileMode.Create);
            int iLen = (int)dwBufSize;
            fs.Write(sData, 0, iLen);
            fs.Close();
        }
        public void Preview(System.Windows.Forms.PictureBox RealPlayWnd)
        {
            if (m_lUserID < 0) return;
            if (m_lRealHandle < 0)
            {
                CHCNetSDK.NET_DVR_PREVIEWINFO lpPreviewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO();
                lpPreviewInfo.hPlayWnd = RealPlayWnd.Handle;//预览窗口
                lpPreviewInfo.lChannel = 1;//预te览的设备通道
                lpPreviewInfo.dwStreamType = 0;//码流类型：0-主码流，1-子码流，2-码流3，3-码流4，以此类推
                lpPreviewInfo.dwLinkMode = 0;//连接方式：0- TCP方式，1- UDP方式，2- 多播方式，3- RTP方式，4-RTP/RTSP，5-RSTP/HTTP 
                lpPreviewInfo.bBlocked = true; //0- 非阻塞取流，1- 阻塞取流
                lpPreviewInfo.dwDisplayBufNum = 1; //播放库播放缓冲区最大缓冲帧数
                lpPreviewInfo.byProtoType = 0;
                lpPreviewInfo.byPreviewMode = 0;
                if (RealData == null){RealData = new CHCNetSDK.REALDATACALLBACK(RealDataCallBack); }//预览实时流回调函数
                IntPtr pUser = new IntPtr();//用户数据
                m_lRealHandle = CHCNetSDK.NET_DVR_RealPlay_V40(m_lUserID, ref lpPreviewInfo, null/*RealData*/, pUser);//打开预览 Start live view 
            }
        }
        public void StartRecord(string sVideoFileName) {
            //强制I帧 Make a I frame
            {
                CHCNetSDK.NET_DVR_PREVIEWINFO lpPreviewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO();
                lpPreviewInfo.lChannel = 1;//预te览的设备通道
                lpPreviewInfo.dwStreamType = 0;//码流类型：0-主码流，1-子码流，2-码流3，3-码流4，以此类推
                lpPreviewInfo.dwLinkMode = 0;//连接方式：0- TCP方式，1- UDP方式，2- 多播方式，3- RTP方式，4-RTP/RTSP，5-RSTP/HTTP 
                lpPreviewInfo.bBlocked = true; //0- 非阻塞取流，1- 阻塞取流
                lpPreviewInfo.dwDisplayBufNum = 1; //播放库播放缓冲区最大缓冲帧数
                lpPreviewInfo.byProtoType = 0;
                lpPreviewInfo.byPreviewMode = 0;
                if (RealData == null) { RealData = new CHCNetSDK.REALDATACALLBACK(RealDataCallBack); }//预览实时流回调函数
                IntPtr pUser = new IntPtr();//用户数据
                m_lRealHandle = CHCNetSDK.NET_DVR_RealPlay_V40(m_lUserID, ref lpPreviewInfo, null/*RealData*/, pUser);//打开预览 Start live view 
            }
            Console.WriteLine(m_lRealHandle);
            CHCNetSDK.NET_DVR_MakeKeyFrame(m_lUserID, 1);
            CHCNetSDK.NET_DVR_SaveRealData(m_lRealHandle, sVideoFileName);//开始录像
        }
        public void StopRecord() {
            CHCNetSDK.NET_DVR_StopSaveRealData(m_lRealHandle); //停止录像
        }
    }
}
