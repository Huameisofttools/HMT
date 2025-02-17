using System;
using System.Collections.Generic;
using System.Collections;
using System.Windows.Forms;
using System.Drawing;

namespace HMT.Kernel
{
    public class AutoControlSize
    {
        private static Queue<Control> MyControlQuery_Init = new Queue<Control>();
        private static ArrayList MyControlInfoList_Init = new ArrayList();
        private static Int32 MainDlg_H_Init;
        private static Int32 MainDlg_W_Init;
        private static Int32 MainDlg_H_Curr;
        private static Int32 MainDlg_W_Curr;
        private struct ControlInfo
        {
            public string ControlName;
            public Int32 Height;
            public Int32 Width;
            public Int32 Loc_X;
            public Int32 Loc_Y;
        }
        private static void FormControlList(Control item)
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
        private static void GetMainFromSize_Init(Form MyForm)
        {
            MainDlg_H_Init = MyForm.Height;
            MainDlg_W_Init = MyForm.Width;
        }
        private static void GetMainFromSize_Curr(Form MyForm)
        {
            MainDlg_H_Curr = MyForm.Height;
            MainDlg_W_Curr = MyForm.Width;
        }
        public static void RegisterFormControl(Form MyForm)
        {
            FormControlList(MyForm);
            GetMainFromSize_Init(MyForm);
        }
        public static void ChangeFormControlSize(Form MyForm)
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
