using WigiDashWidgetFramework;
using WigiDashWidgetFramework.WidgetUtility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Controls;

using System;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Timers;
using System.Diagnostics;
using System.Security.Permissions;
using System.Reflection;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Reflection.Emit;

namespace Task_MgrWidget
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct My_Processor_Core
    {
        public int Efficiency_Class_size;
        public int GroupCount;
        public int Group_size;
        public fixed int Node_num[512];
        public unsafe fixed int Efficiency_Class[512];
        public unsafe fixed int Hyperthreaded[512];
        public unsafe fixed ulong Processor_Mask[512];
        public int num_smallcores;
        public int num_bigcores;
        public unsafe fixed ushort get_Group[512];
        public int threads;
        public unsafe fixed int slice_number[512];
        public unsafe fixed int cluster_number[512];
        public unsafe fixed byte thread_usage_vec[512];
    }
    public struct grid_point_struct
    {
        public Point[]grid_ptrs;
        public int x_offset;
        public int y_offset;
        public int start_x;
        public int end_x;
        public int start_y;
        public int end_y;
        public int x_range;
        public int y_range;
        public int label_x;
        public int label_y;
    }
    public struct grid_line_struct
    {
        public Point[] grid_ptrs;
    }
    public partial class Task_MgrWidgetInstance : IWidgetInstance
    {
        [DllImport("taskmgr.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void initialize_libs();
        [DllImport("taskmgr.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void get_my_processor(ref My_Processor_Core p); 
        [DllImport("taskmgr.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void get_core_usage(byte* p);
        [DllImport("taskmgr.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void deinitialize_libs();

        [DllImport("taskmgr32.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void initialize_libs32();
        [DllImport("taskmgr32.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void get_my_processor32(ref My_Processor_Core p);
        [DllImport("taskmgr32.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void get_core_usage32(byte* p);
        [DllImport("taskmgr32.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void deinitialize_libs32();

        public int x_pos = 0;
        public int y_pos = 0;
        public void RequestUpdate()
        {
            if (drawing_mutex.WaitOne(1000))
            {
                DrawCores();
                drawing_mutex.ReleaseMutex();
            }
        }

        public void ClickEvent(ClickType click_type, int x, int y)
        {
            x_pos = x;
            y_pos = y;
            parent.WidgetManager.OnTriggerOccurred(my_clicked_trigger_guid);            
            
        }

        public System.Windows.Controls.UserControl GetSettingsControl() => new SettingsControl(this);

        public void Dispose()
        {
            run_task = false;
            pause_task = false;
            if (is32bit)
            {
                deinitialize_libs32();
            }
            else
                deinitialize_libs();
        }

        public void EnterSleep()
        {
            pause_task = true;
        }

        public void ExitSleep()
        {
            pause_task = false;
            timestamp_last = DateTime.MinValue;
        }

        // Class specific
        private Thread task_thread;
        private volatile bool run_task;
        private volatile bool pause_task;

        public Font DrawFontDate;
        public Font UserFontDate;
        public Font DrawFontTime;
        public Font UserFontTime;
        Bitmap BitmapCurrent;
        private DateTime timestamp_last;

        Mutex drawing_mutex = new Mutex();

        public bool time_24h = true;

        private Guid clicked_trigger_guid = new Guid("F6228B98-B94B-4088-8CA6-484A36436E2A");
        private Guid me_toggle_guid = new Guid("cbd0c64c-13ff-49b6-a2f3-9f7f2eb8141c");
        private Guid my_clicked_trigger_guid = new Guid("b5c97a2c-9a9c-4e5f-be31-575d1ffcfb07");

        public Color DrawBackColor;
        public Color UserBackColor;
        public Color DrawForeColor;
        public Color UserForeColor;
        public bool UseGlobal = false;
        public My_Processor_Core mecores = new My_Processor_Core();


        public int num_ticks_elapsed = 0;

        public int num_core_grids = 0;
        public Color LineColor;

        public grid_point_struct[] grid_struct_vec;
        public grid_line_struct grid_line_vec;
        public int num_gridlines = 0;
        public bool is32bit = false;
        public enum PAGE_STATE
        {
            main_page,
           others
        }
        public PAGE_STATE current_page = PAGE_STATE.main_page;

        public unsafe void initialize_grid()
        {
            int num_horizontal_boxes = 1;
            int num_vertical_boxes = 1;
            switch (num_core_grids)
            {
                case 2:
                    num_horizontal_boxes = 2;
                    break;
                case 3:
                case 4:
                    num_horizontal_boxes = 2;
                    num_vertical_boxes = 2;
                    break;
                case 5:
                case 6:
                    num_horizontal_boxes = 3;
                    num_vertical_boxes = 2;
                    break;
                case 7:
                case 8:
                    num_horizontal_boxes = 4;
                    num_vertical_boxes = 2;
                    break;
                case 9:
                case 10:
                case 11:
                case 12:
                    num_horizontal_boxes = 4;
                    num_vertical_boxes = 3;
                    break;
                case 13:
                case 14:
                case 15:
                case 16:
                    num_horizontal_boxes = 4;
                    num_vertical_boxes = 4;
                    break;
                case 17:
                case 18:
                case 19:
                case 20:
                    num_horizontal_boxes = 5;
                    num_vertical_boxes = 4;
                    break;
                case 21:
                case 22:
                case 23:
                case 24:
                case 25:
                    num_horizontal_boxes = 5;
                    num_vertical_boxes = 5;
                    break;
                case 26:
                case 27:
                case 28:
                case 29:
                case 30:
                    num_horizontal_boxes = 6;
                    num_vertical_boxes = 5;
                    break;
                case 31:
                case 32:
                case 33:
                case 34:
                case 35:
                case 36:
                    num_horizontal_boxes = 6;
                    num_vertical_boxes = 6;
                    break;
                default:
                    break;
            }

            int spacer = 2;
            int width_for_each = (BitmapCurrent.Width / num_horizontal_boxes)- spacer;
            int height_for_each = (BitmapCurrent.Height / num_vertical_boxes) - spacer-1;

            int num_vertical_lines = num_vertical_boxes - 1;
            int num_horizontal_lines = num_horizontal_boxes - 1;
            
            int index = 0;
            int startx = width_for_each + spacer;
            grid_line_vec.grid_ptrs = new Point[512];
            int nlines = 0;
            while (index< num_horizontal_lines)
            {
                Point mept= new Point(startx, 0);
                Point mept2 = new Point(startx, BitmapCurrent.Height);
                grid_line_vec.grid_ptrs[nlines] = mept;//.Append(mept);
                grid_line_vec.grid_ptrs[nlines + 1] = mept2;//.Append(mept2);
                num_gridlines++;
                nlines += 2;
                index++;
                startx += (width_for_each + spacer);
            }
            index = 0;
            int starty = height_for_each + spacer;
            while (index < num_vertical_lines)
            {
                Point mept = new Point(0, starty);
                Point mept2 = new Point(BitmapCurrent.Width, starty);
                grid_line_vec.grid_ptrs[nlines] = mept;//.Append(mept);
                grid_line_vec.grid_ptrs[nlines + 1] = mept2;//.Append(mept2);
                num_gridlines++;
                nlines += 2;
                index++;
                starty += (height_for_each + spacer);
            }
            index = 0;
            startx = 0;
            starty = 0;           

            for (int row=0;row<= num_vertical_lines; row++)
            {
                bool done = false;
                for (int col = 0; col <= num_horizontal_lines; col++)
                {
                    grid_struct_vec[index].start_y = starty;
                    grid_struct_vec[index].end_y = starty+ height_for_each;
                    grid_struct_vec[index].start_x = startx;
                    grid_struct_vec[index].end_x = startx + width_for_each;
                    grid_struct_vec[index].y_range = height_for_each;
                    grid_struct_vec[index].x_range = width_for_each;
                    grid_struct_vec[index].grid_ptrs = new Point[width_for_each+20];
                    grid_struct_vec[index].grid_ptrs[0] = new Point(startx, grid_struct_vec[index].end_y);
                    grid_struct_vec[index].label_x = startx + 2;
                    grid_struct_vec[index].label_y = starty + 2;
                    startx += (width_for_each+3);
                    index++;
                    if(index>=num_core_grids)
                    {
                        done = true;
                        break;
                    }
                }
                if (done)
                    break;
                startx = 0;
                starty += (height_for_each+3);
            }


        }

        public unsafe Task_MgrWidgetInstance(Task_MgrWidgetServer parent, WidgetSize widget_size, Guid instance_guid)
        {
            this.parent = parent;
            this.WidgetSize = widget_size;
            this.Guid = instance_guid;
            if (widget_size.Equals(2, 1))
            {
                DrawFontDate = new Font("Verdana", 18, FontStyle.Bold);
                DrawFontTime = new Font("Basic Square 7", 56);
            }
            else
            {
                DrawFontDate = new Font("Verdana", 14, FontStyle.Bold);
                DrawFontTime = new Font("Basic Square 7", 30);
            }
            LineColor = Color.LightPink;
            
            BitmapCurrent = new Bitmap(widget_size.ToSize().Width, widget_size.ToSize().Height);
            if (sizeof(IntPtr) == 4)
            {
                is32bit = true;
                initialize_libs32();
                get_my_processor32(ref mecores);
            }
            else
            {
                initialize_libs();
                get_my_processor(ref mecores);
            }
            
            num_core_grids = mecores.Efficiency_Class_size;
            if (num_core_grids > 32)
                num_core_grids = 32;
            grid_struct_vec = new grid_point_struct[num_core_grids];
            grid_line_vec = new grid_line_struct();
            initialize_grid();
            
            LoadSettings();

            UpdateSettings();

            // Register widget clicked
            parent.WidgetManager.RegisterTrigger(this, my_clicked_trigger_guid, "Clicked");

            // Register toggle time
            parent.WidgetManager.RegisterAction(this, me_toggle_guid, "Toggle me");

            // Register for action events
            parent.WidgetManager.ActionRequested += WidgetManager_ActionRequested;

            parent.WidgetManager.GlobalThemeUpdated += WidgetManager_GlobalThemeUpdated;

            // Start thread
            ThreadStart thread_start = new ThreadStart(UpdateTask);
            task_thread = new Thread(thread_start);
            task_thread.IsBackground = true;
            run_task = true;
            pause_task = false;
            timestamp_last = DateTime.MinValue;            
            task_thread.Start();
        }

        private void WidgetManager_GlobalThemeUpdated()
        {
            if (UseGlobal)
            {
                UpdateSettings();
            }
        }

        private void WidgetManager_ActionRequested(Guid action_guid) {
            if (action_guid == me_toggle_guid)
            {                
                if (drawing_mutex.WaitOne(1000))
                {
                    UpdateWidget();
                    drawing_mutex.ReleaseMutex();
                }
            }
        }

        private unsafe void DrawCores()
        {
            if (drawing_mutex.WaitOne(1000))
            {
                using (Graphics g = Graphics.FromImage(BitmapCurrent))
                {
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit; 
                    g.Clear(DrawBackColor);
                    Pen ppen = new Pen(LineColor);
                    int index = 0;
                    for (int i=0;i<num_gridlines;i++)
                    {
                        g.DrawLine(ppen, grid_line_vec.grid_ptrs[index].X, grid_line_vec.grid_ptrs[index].Y, grid_line_vec.grid_ptrs[index+1].X, grid_line_vec.grid_ptrs[index + 1].Y);
                        
                        index += 2;
                    }
                    Pen ppen2 = new Pen(DrawForeColor);
                    
                    Brush textBrush = new SolidBrush(Color.PaleVioletRed);
                   
                    if (num_ticks_elapsed > 4)
                    {
                        for (int i = 0; i < num_core_grids; i++)
                        {
                            g.DrawCurve(ppen2, grid_struct_vec[i].grid_ptrs, 0, num_ticks_elapsed, 0.0f);
                            string labe = "c"+i.ToString();
                            g.DrawString(labe, DrawFontDate, textBrush, grid_struct_vec[i].label_x, grid_struct_vec[i].label_y);
                        }
                    }
                }
                UpdateWidget();
                drawing_mutex.ReleaseMutex();
            }
        }

        private void UpdateWidget()
        {
            WidgetUpdatedEventArgs e = new WidgetUpdatedEventArgs();
            e.WidgetBitmap = BitmapCurrent;
            e.WaitMax = 1000;
            WidgetUpdated?.Invoke(this, e);
        }
        private unsafe void grid_struct_add_pts()
        {
            int index = 0;
            while(index<num_core_grids)
            {
                double load = mecores.thread_usage_vec[index];
                load = (load / 100.0) * (double)grid_struct_vec[index].y_range;
                int y= grid_struct_vec[index].end_y-(int)load;
                grid_struct_vec[index].grid_ptrs[num_ticks_elapsed] = new Point(num_ticks_elapsed+ grid_struct_vec[index].start_x, y);
                index++;
            }
        }
        private unsafe void grid_struct_move_forward()
        {
            int index = 0;
            while (index < num_core_grids)
            {
                for(int i=1;i<= num_ticks_elapsed;i++)
                {
                    grid_struct_vec[index].grid_ptrs[i - 1].Y = grid_struct_vec[index].grid_ptrs[i].Y;
                }
                double load = mecores.thread_usage_vec[index];
                load = (load / 100.0) * (double)grid_struct_vec[index].y_range;
                int y = grid_struct_vec[index].end_y - (int)load;
                grid_struct_vec[index].grid_ptrs[num_ticks_elapsed].X = num_ticks_elapsed + grid_struct_vec[index].start_x;
                grid_struct_vec[index].grid_ptrs[num_ticks_elapsed].Y = y;
                index++;
            }
        }
        private unsafe void Update_core_usage()
        {
            fixed (byte* p = mecores.thread_usage_vec)
            {
                if(is32bit)
                    get_core_usage32(p);
                else
                    get_core_usage(p);
            }
            if (num_ticks_elapsed < grid_struct_vec[0].x_range)
            {
                num_ticks_elapsed++;
                grid_struct_add_pts();                
            }
            else
            {
                grid_struct_move_forward();
            }
            DrawCores();
        }
        private void UpdateTask()
        {             
            while (run_task)
            {
                if (current_page == PAGE_STATE.main_page)
                    Update_core_usage();
                if (!run_task) return;
                    Thread.Sleep(100);
            }
        }

        public void SaveSettings()
        {
            // Save setting
            parent.WidgetManager.StoreSetting(this, nameof(UseGlobal), UseGlobal.ToString());
            parent.WidgetManager.StoreSetting(this, nameof(UserBackColor), ColorTranslator.ToHtml(UserBackColor));
            parent.WidgetManager.StoreSetting(this, nameof(UserForeColor), ColorTranslator.ToHtml(UserForeColor));
            parent.WidgetManager.StoreSetting(this, nameof(UserFontDate), new FontConverter().ConvertToInvariantString(UserFontDate));
            parent.WidgetManager.StoreSetting(this, nameof(UserFontTime), new FontConverter().ConvertToInvariantString(UserFontTime));
        }

        public void LoadSettings()
        {
            if (parent.WidgetManager.LoadSetting(this, nameof (UseGlobal), out string useGlobalStr))
            {
                UseGlobal = bool.Parse(useGlobalStr);
            } else
            {
                UseGlobal = parent.WidgetManager.PreferGlobalTheme;
            }

            if (parent.WidgetManager.LoadSetting(this, nameof(UserBackColor), out string bgTintStr))
            {
                UserBackColor = ColorTranslator.FromHtml(bgTintStr);
            } else
            {
                Random rnd = new Random();
                UserBackColor = Color.FromArgb(rnd.Next(0, 150), rnd.Next(0, 150), rnd.Next(0, 150));
            }

            if (parent.WidgetManager.LoadSetting(this, nameof(UserForeColor), out string fgColorStr))
            {
                UserForeColor = ColorTranslator.FromHtml(fgColorStr);
            }
            else
            {
                UserForeColor = Color.FromArgb(255 - UserBackColor.R, 255 - UserBackColor.G, 255 - UserBackColor.B);
            }

            if (parent.WidgetManager.LoadSetting(this, nameof(UserFontDate), out var strDateFont))
            {
                UserFontDate = new FontConverter().ConvertFromInvariantString(strDateFont) as Font;
            } else
            {
                UserFontDate = DrawFontDate;
            }

            if (parent.WidgetManager.LoadSetting(this, nameof(UserFontTime), out var strTimeFont))
            {
                UserFontTime = new FontConverter().ConvertFromInvariantString(strTimeFont) as Font;
            } else
            {
                UserFontTime = DrawFontTime;
            }
        }

        public void UpdateSettings()
        {
            if (UseGlobal)
            {
                DrawBackColor = parent.WidgetManager.GlobalWidgetTheme.PrimaryBgColor;
                DrawForeColor = parent.WidgetManager.GlobalWidgetTheme.PrimaryFgColor;
                DrawFontDate = new Font(parent.WidgetManager.GlobalWidgetTheme.SecondaryFont.FontFamily, DrawFontDate.Size, DrawFontDate.Style);
                DrawFontTime = new Font(parent.WidgetManager.GlobalWidgetTheme.PrimaryFont.FontFamily, DrawFontTime.Size, DrawFontTime.Style);
            }
            else
            {
                DrawBackColor = UserBackColor;
                DrawForeColor = UserForeColor;
                DrawFontDate = UserFontDate;
                DrawFontTime = UserFontTime;
            }

            RequestUpdate();
        }
    }
}

