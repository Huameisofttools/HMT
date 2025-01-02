using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;
using System.Drawing;

namespace HMT.Kernel
{
    public class AutoControlSize
    {
        private static Queue<Control> MyControlQuery_Init = new Queue<Control>();//初始化控件集合队列
        private static ArrayList MyControlInfoList_Init = new ArrayList();//初始化控件参数集合
        private static Int32 MainDlg_H_Init;//初始化主对话框参数
        private static Int32 MainDlg_W_Init;//初始化主对话框参数
        private static Int32 MainDlg_H_Curr;//当前主对话框参数
        private static Int32 MainDlg_W_Curr;//当前主对话框参数
        private struct ControlInfo//控件参数结构体
        {
            public string ControlName;
            public Int32 Height;
            public Int32 Width;
            public Int32 Loc_X;
            public Int32 Loc_Y;
        }
        private static void FormControlList(Control item)//递归遍历控件
        {
            for (int i = 0; i < item.Controls.Count; i++)
            {
                if (item.Controls[i].HasChildren)
                {
                    FormControlList(item.Controls[i]);
                }
                ControlInfo Node = new ControlInfo();
                Node.ControlName = item.Controls[i].Name;
                Node.Height = item.Controls[i].Height;
                Node.Width = item.Controls[i].Width;
                Node.Loc_X = item.Controls[i].Location.X;
                Node.Loc_Y = item.Controls[i].Location.Y;
                MyControlInfoList_Init.Add(Node);
                MyControlQuery_Init.Enqueue(item.Controls[i]);
            }
        }
        private static void GetMainFromSize_Init(Form MyForm)//获取初始化对话框参数
        {
            MainDlg_H_Init = MyForm.Height;
            MainDlg_W_Init = MyForm.Width;
        }
        private static void GetMainFromSize_Curr(Form MyForm)//获取当前对话框参数
        {
            MainDlg_H_Curr = MyForm.Height;
            MainDlg_W_Curr = MyForm.Width;
        }
        public static void RegisterFormControl(Form MyForm)//注册对话框所以控件
        {
            FormControlList(MyForm);
            GetMainFromSize_Init(MyForm);
        }
        public static void ChangeFormControlSize(Form MyForm)//使能AutoSize
        {
            GetMainFromSize_Curr(MyForm);
            Control myQuery;
            Queue<Control> ControlQuery = new Queue<Control>(MyControlQuery_Init);
            ControlInfo Node = new ControlInfo();
            Int32 i = 0;
            Int32 count = ControlQuery.Count;
            for (i = 0; i < count; i++)
            {
                myQuery = ControlQuery.Dequeue();
                Node = (ControlInfo)MyControlInfoList_Init[i];
                myQuery.Height = (Int32)(Node.Height * (MainDlg_H_Curr / (double)MainDlg_H_Init));
                myQuery.Width = (Int32)(Node.Width * (MainDlg_W_Curr / (double)MainDlg_W_Init));
                myQuery.Location = new Point(
                    (Int32)(Node.Loc_X * (MainDlg_W_Curr / (double)MainDlg_W_Init)),
                    (Int32)(Node.Loc_Y * (MainDlg_H_Curr / (double)MainDlg_H_Init)));
            }
        }
    }
}
