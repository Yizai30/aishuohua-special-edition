using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnimGenerator
{

   

    [Serializable]
    public class KeyFrame
    {
        /// <summary>
        /// 0 出现，1 消失，2 移动或播放动画，3 音频播放和停止，4 背景图切换，5字幕，6对话框
        /// </summary>
        
        public int action { get; set; }

        /// <summary>
        /// 0 道具，1 人，2 四脚爬行，3 飞行动物
        /// </summary>
        public int type { get; set; }

        /// <summary>
        /// action == 0 or 1 or 2 时，用以索引预制体的名字
        /// action == 3 时，指示要播放的音频在 Resources 文件夹下的路径，不包含后缀名
        /// action == 4 时，指示要切换的图片在 Resources 文件夹下的路径，不包含后缀名
        /// action == 6 时，指示要显示对话框的角色
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 场景中具体游戏对象的名字
        /// </summary>
        public string goname { get; set; }

        /// <summary>
        /// 该动作开始执行的时间戳，单位是秒(s)
        /// </summary>
        public float timestamp { get; set; }

        // 下列为物体在变化过程中的位置、旋转和缩放。其中 action == 1 时，仅 startxxx 有意义；action == 2 时都有意义。
        // 2D 模式：
        //       摄像机正交投影，z 轴指示物体前后关系。背景图片 z 坐标为 10，摄像机 z 坐标为 -10，观察方向为 z 轴正方向，物体的 z 坐标越大，越靠后。
        //       画面左下角在 xy 平面的投影为 (-16, -9)，画面右上角在 xy 平面的投影为 (16, 9)
        // 3D 模式：
        //       请将摄像机切换到透视投影，摄像机位置为 (0, 0, -10)，观察方向为 (0, 0, 1)
        public List<float> startpos { get; set; }
        public List<float> endpos { get; set; }
        public List<float> startrotation { get; set; }
        public List<float> endrotation { get; set; }
        public List<float> startscale { get; set; }
        public List<float> endscale { get; set; }

        /// <summary>
        /// 该动作的持续时间，在 action == 2/3/5 时有意义。
        /// </summary>
        public float duration { get; set; }

        /// <summary>
        /// action == 2 时，2D 模式下指示播放动画的名字，3D 模式下指示要设置的状态机 trigger 名称
        /// action == 5 or 6 时，指示字幕或对话框的内容。
        /// </summary>
        public string content { get; set; }

        /// <summary>
        /// 动画（action == 2）或音频（action == 3）的循环方式
        /// >0 时为循环次数（与 duration 一起作用可改变动画播放速度），
        /// =0 时，若 action == 2 无意义，action == 3 表示音频播放时长跟随 duration 字段
        /// =-1 时以原速度表示无限循环
        /// </summary>
        public int loop { get; set; }
        //public string weather;

        //public Dictionary<string,string> clothes;
        //public List<string> partNameList = new List<string>();
        //public List<string> partPreList = new List<string>();

        public List<float> color { get; set; }

        public KeyFrame(int action, int type, string name, string goname,float timestamp,
            List<float> startpos, List<float> endpos,
            List<float> startrotation, List<float> endrotation,
            List<float> startscale, List<float> endscale,
            float duration, string animation, int loop,List<float>color)
        {
            this.action = action;
            this.type = type;
            this.name = name;
            this.goname = goname;
            this.timestamp = timestamp;
            this.startpos = startpos;
            this.endpos = endpos;
            this.startrotation = startrotation;
            this.endrotation = endrotation;
            this.startscale = startscale;
            this.endscale = endscale;
            this.duration = duration;
            this.content = animation;
            this.loop = loop;
            this.color = color;
            //this.weather = weather;
            //this.clothes = clothes;
        }

       

        public Vector3 getVec3(List<float> list)
        {
            Vector3 _startpos;
            _startpos = new Vector3(list[0], list[1], list[2]);
            return _startpos;
        }

        public Quaternion getQuaternion(List<float> list)
        {
            Quaternion _startrotation;

            _startrotation = Quaternion.Euler(list[0], list[1], list[2]);

            return _startrotation;
        }

      

       
    }
}

