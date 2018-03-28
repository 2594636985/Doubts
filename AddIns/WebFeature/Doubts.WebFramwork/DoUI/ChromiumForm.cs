using Chromium.WebBrowser;
using Doubts.WebFramework.Internal;
using Doubts.WebFramework.Internal.Imports;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Doubts.WebFramework.DoUI
{
    public class ChromiumForm : Form
    {
        private string initialUrl;
        private ChromiumWebBrowser browser;
        private PictureBox splashPicture;
        private Region draggableRegion = null;
        private BrowserWidgetMessageInterceptor messageInterceptor;
        private float scaleFactor = 1.0f;
        private int borderSize = 1;

        private FormNCAreaDecorator formNCAreaDecorator = null;

        private bool isSplashShown = true;
        private bool isFirstTimeSplashShown = true;
        private bool isResizable = true;

        protected readonly bool IsDesignMode = LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        protected IntPtr FormHandle { get; private set; }
        protected IntPtr BrowserHandle { get; private set; }

        protected FormNCAreaDecorator FormNonclientAreaDecorator
        {
            get
            {
                return formNCAreaDecorator;
            }
        }

        public bool IsLoading { get { return this.browser.IsLoading; } }

        public bool CanGoBack { get { return this.browser.CanGoBack; } }

        public bool CanGoForward { get { return this.browser.CanGoForward; } }


        private int CornerAreaSize
        {
            get
            {
                return borderSize < 3 ? 3 : borderSize;
            }
        }

        public int BorderSize
        {
            get
            {
                return borderSize;
            }
            set
            {
                borderSize = value;
                if (!IsDesignMode)
                {
                    FormNonclientAreaDecorator.BorderSize = borderSize;
                }
            }
        }

        public bool Resizable
        {
            get
            {
                return this.isResizable;
            }
            set
            {
                this.isResizable = value;
            }
        }

        /// <summary>
        /// 加载图片
        /// </summary>
        public Image SplashImage
        {
            get
            {
                return splashPicture.BackgroundImage;
            }
            set
            {
                splashPicture.BackgroundImage = value;
            }
        }

        public ImageLayout SplashImageLayout
        {
            get
            {
                return splashPicture.BackgroundImageLayout;
            }
            set
            {
                splashPicture.BackgroundImageLayout = value;
            }
        }

        public Color SplashBackColor
        {
            get
            {
                return splashPicture.BackColor;
            }
            set
            {
                splashPicture.BackColor = value;
            }
        }



        public ChromiumForm() : this(null)
        {

        }

        public ChromiumForm(string initialUrl)
        {
            this.initialUrl = initialUrl;

            this.splashPicture = new PictureBox()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            if (!this.IsDesignMode)
            {
                this.scaleFactor = 1.0f / User32.GetOriginalDeviceScaleFactor(FormHandle);

                this.Controls.Add(splashPicture);

                this.splashPicture.BringToFront();

                this.InitializeChromium(this.initialUrl);
            }


        }
        protected void InitializeChromium(string initialUrl)
        {
            if (string.IsNullOrEmpty(initialUrl))
            {
                this.browser = new ChromiumWebBrowser();
            }
            else
            {
                this.browser = new ChromiumWebBrowser(initialUrl);
            }

            this.browser.Dock = DockStyle.Fill;
            this.browser.RemoteCallbackInvokeMode = JSInvokeMode.Inherit;

            this.Controls.Add(this.browser);

            this.BrowserHandle = this.browser.Handle;

            this.browser.BrowserCreated += (sender, args) =>
            {
                AttachInterceptorToChromiumBrowser();
            };

            this.browser.LifeSpanHandler.OnBeforePopup += (sender, args) =>
            {

            };

            this.browser.DragHandler.OnDraggableRegionsChanged += (sender, args) =>
            {
                var regions = args.Regions;

                if (regions.Length > 0)
                {
                    foreach (var region in regions)
                    {
                        var rect = new Rectangle(region.Bounds.X, region.Bounds.Y, region.Bounds.Width, region.Bounds.Height);

                        if (this.draggableRegion == null)
                        {
                            this.draggableRegion = new Region(rect);
                        }
                        else
                        {
                            if (region.Draggable)
                            {
                                this.draggableRegion.Union(rect);
                            }
                            else
                            {
                                this.draggableRegion.Exclude(rect);
                            }
                        }
                    }
                }

            };

            this.browser.DragHandler.OnDragEnter += (s, e) =>
            {
                e.SetReturnValue(true);
            };

            this.browser.LoadHandler.OnLoadEnd += (sender, args) =>
            {
                HideInitialSplash();
            };

            this.browser.LoadHandler.OnLoadError += (sender, args) =>
            {
                HideInitialSplash();
            };
        }

        #region Private

        private void HideInitialSplash()
        {
            if (isFirstTimeSplashShown && isSplashShown)
            {
                HideSplash();

                isFirstTimeSplashShown = false;
                isSplashShown = false;
            }
        }

        private void HideSplash()
        {
            this.UpdateUI(() =>
            {
                this.splashPicture.Hide();
                this.splashPicture.SendToBack();
            });
        }

        private void AttachInterceptorToChromiumBrowser()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    while (true)
                    {
                        IntPtr chromeWidgetHostHandle = IntPtr.Zero;

                        if (BrowserWidgetHandleFinder.TryFindHandle(this.BrowserHandle, out chromeWidgetHostHandle))
                        {
                            messageInterceptor = new BrowserWidgetMessageInterceptor(browser, chromeWidgetHostHandle, OnWebBroswerMessage);
                            break;
                        }
                        else
                        {
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                }
                catch
                {

                }
            });
        }

        private void SetCursor(HitTest mode)
        {


            IntPtr handle = IntPtr.Zero;

            switch (mode)
            {
                case HitTest.HTTOP:
                case HitTest.HTBOTTOM:
                    handle = User32.LoadCursor(IntPtr.Zero, (int)IdcStandardCursors.IDC_SIZENS);
                    break;
                case HitTest.HTLEFT:
                case HitTest.HTRIGHT:
                    handle = User32.LoadCursor(IntPtr.Zero, (int)IdcStandardCursors.IDC_SIZEWE);
                    break;
                case HitTest.HTTOPLEFT:
                case HitTest.HTBOTTOMRIGHT:
                    handle = User32.LoadCursor(IntPtr.Zero, (int)IdcStandardCursors.IDC_SIZENWSE);
                    break;
                case HitTest.HTTOPRIGHT:
                case HitTest.HTBOTTOMLEFT:
                    handle = User32.LoadCursor(IntPtr.Zero, (int)IdcStandardCursors.IDC_SIZENESW);
                    break;
            }

            if (handle != IntPtr.Zero)
            {
                User32.SetCursor(handle);
            }
        }

        #endregion

        #region Protected
        protected virtual bool OnWebBroswerMessage(Message message)
        {

            if (message.Msg == (int)WindowsMessages.WM_MOUSEACTIVATE)
            {
                var topLevelWindowHandle = message.WParam;
                User32.PostMessage(topLevelWindowHandle, (int)WindowsMessages.WM_SETFOCUS, IntPtr.Zero, IntPtr.Zero);
                User32.SendMessage(topLevelWindowHandle, (int)WindowsMessages.WM_NCLBUTTONDOWN, IntPtr.Zero, IntPtr.Zero);
            }


            if (message.Msg == (int)WindowsMessages.WM_LBUTTONDOWN)
            {
                var x = (int)User32.LoWord(message.LParam);
                var y = (int)User32.HiWord(message.LParam);

                var sx = (int)((int)User32.LoWord(message.LParam) * scaleFactor);
                var sy = (int)((int)User32.HiWord(message.LParam) * scaleFactor);

                var ax = x;
                var ay = y;

                if (scaleFactor != 1.0f)
                {
                    ax = sx;
                    ay = sy;
                }


                var dragable = (draggableRegion != null && draggableRegion.IsVisible(new Point(sx, sy)));

                var dir = GetSizeMode(new POINT(x, y));

                if (dir != HitTest.HTCLIENT/* && BorderSize == 0*/)
                {
                    User32.PostMessage(FormHandle, (uint)DefMessages.WM_CEF_RESIZE_CLIENT, (IntPtr)dir, message.LParam);
                    return true;
                }
                else if (dragable)
                {
                    User32.PostMessage(FormHandle, (uint)DefMessages.WM_CEF_DRAG_APP, message.WParam, message.LParam);
                    return true;
                }
            }

            if (message.Msg == (int)WindowsMessages.WM_LBUTTONDBLCLK && Resizable)
            {
                var x = (int)User32.LoWord(message.LParam);
                var y = (int)User32.HiWord(message.LParam);

                var sx = (int)((int)User32.LoWord(message.LParam) * scaleFactor);
                var sy = (int)((int)User32.HiWord(message.LParam) * scaleFactor);

                var ax = x;
                var ay = y;

                if (scaleFactor != 1.0f)
                {
                    ax = sx;
                    ay = sy;
                }

                var dragable = (draggableRegion != null && draggableRegion.IsVisible(new Point(sx, sy)));

                if (dragable)
                {
                    User32.PostMessage(FormHandle, (uint)DefMessages.WM_CEF_TITLEBAR_LBUTTONDBCLICK, message.WParam, message.LParam);

                    return true;
                }

            }

            if (message.Msg == (int)WindowsMessages.WM_MOUSEMOVE/* &&  BorderSize == 0*/)
            {

                var x = (int)User32.LoWord(message.LParam);
                var y = (int)User32.HiWord(message.LParam);

                var sx = (int)((int)User32.LoWord(message.LParam) * scaleFactor);
                var sy = (int)((int)User32.HiWord(message.LParam) * scaleFactor);

                var ax = x;
                var ay = y;

                if (scaleFactor != 1.0f)
                {
                    ax = sx;
                    ay = sy;
                }


                var dragable = (draggableRegion != null && draggableRegion.IsVisible(new Point(sx, sy)));

                Debug.WriteLine($"x:{x}\ty:{y}\t|\tax:{ax}\tay:{ay}");


                if (Resizable)
                {
                    var dir = GetSizeMode(new POINT(x, y));


                    if (dir != HitTest.HTCLIENT)
                    {
                        User32.PostMessage(FormHandle, (uint)DefMessages.WM_CEF_EDGE_MOVE, (IntPtr)dir, message.LParam);
                        return true;
                    }

                }

                User32.SendMessage(FormHandle, (uint)WindowsMessages.WM_MOUSEMOVE, message.WParam, message.LParam);

            }


            return false;

        }

        private HitTest GetSizeMode(POINT point)
        {
            HitTest mode = HitTest.HTCLIENT;

            int x = point.x, y = point.y;

            if (WindowState == FormWindowState.Normal)
            {
                if (x < CornerAreaSize & y < CornerAreaSize)
                {
                    mode = HitTest.HTTOPLEFT;
                }
                else if (x < CornerAreaSize & y + CornerAreaSize > this.Height - CornerAreaSize)
                {
                    mode = HitTest.HTBOTTOMLEFT;

                }
                else if (x + CornerAreaSize > this.Width - CornerAreaSize & y + CornerAreaSize > this.Height - CornerAreaSize)
                {
                    mode = HitTest.HTBOTTOMRIGHT;

                }
                else if (x + CornerAreaSize > this.Width - CornerAreaSize & y < CornerAreaSize)
                {
                    mode = HitTest.HTTOPRIGHT;

                }
                else if (x < CornerAreaSize)
                {
                    mode = HitTest.HTLEFT;

                }
                else if (x + CornerAreaSize > this.Width - CornerAreaSize)
                {
                    mode = HitTest.HTRIGHT;

                }
                else if (y < CornerAreaSize)
                {
                    mode = HitTest.HTTOP;

                }
                else if (y + CornerAreaSize > this.Height - CornerAreaSize)
                {
                    mode = HitTest.HTBOTTOM;

                }

            }


            return mode;
        }
        #endregion

        #region Override
        protected override void OnHandleCreated(EventArgs e)
        {
            FormHandle = this.Handle;
            base.OnHandleCreated(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            messageInterceptor?.ReleaseHandle();
            messageInterceptor?.DestroyHandle();
            messageInterceptor = null;

            browser.Dispose();

            base.OnClosed(e);
        }



        protected override void WndProc(ref Message m)
        {
            if (!IsDesignMode)
            {

                switch (m.Msg)
                {
                    case (int)WindowsMessages.WM_SHOWWINDOW:
                        {


                            if (StartPosition == FormStartPosition.CenterParent && Owner != null)
                            {
                                Location = new Point(Owner.Location.X + Owner.Width / 2 - Width / 2,
                                Owner.Location.Y + Owner.Height / 2 - Height / 2);


                            }
                            else if (StartPosition == FormStartPosition.CenterScreen || (StartPosition == FormStartPosition.CenterParent && Owner == null))
                            {
                                var currentScreen = Screen.FromHandle(this.Handle);
                                Location = new Point(currentScreen.WorkingArea.Left + (currentScreen.WorkingArea.Width / 2 - this.Width / 2), currentScreen.WorkingArea.Top + (currentScreen.WorkingArea.Height / 2 - this.Height / 2));

                            }

                            Activate();
                            BringToFront();

                            base.WndProc(ref m);
                        }
                        break;
                    case (int)WindowsMessages.WM_MOVE:
                        {

                            browser?.BrowserHost?.NotifyScreenInfoChanged();


                            base.WndProc(ref m);
                        }
                        break;
                    default:
                        {
                            base.WndProc(ref m);
                        }
                        break;
                }

            }
            else
            {
                base.WndProc(ref m);
            }



        }

        protected override void DefWndProc(ref Message m)
        {
            if (m.Msg == (int)DefMessages.WM_CEF_TITLEBAR_LBUTTONDBCLICK)
            {
                User32.ReleaseCapture();

                if (WindowState == FormWindowState.Maximized)
                {
                    User32.SendMessage(FormHandle, (uint)WindowsMessages.WM_SYSCOMMAND, (IntPtr)SystemCommandFlags.SC_RESTORE, IntPtr.Zero);
                }
                else
                {
                    User32.SendMessage(FormHandle, (uint)WindowsMessages.WM_SYSCOMMAND, (IntPtr)SystemCommandFlags.SC_MAXIMIZE, IntPtr.Zero);
                }
            }

            if (m.Msg == (int)DefMessages.WM_CEF_DRAG_APP && !(FormBorderStyle == FormBorderStyle.None && WindowState == FormWindowState.Maximized))
            {
                User32.ReleaseCapture();
                User32.SendMessage(Handle, (uint)WindowsMessages.WM_NCLBUTTONDOWN, (IntPtr)HitTest.HTCAPTION, (IntPtr)0);
            }
            if (m.Msg == (int)DefMessages.WM_CEF_RESIZE_CLIENT && Resizable && WindowState == FormWindowState.Normal)
            {
                User32.ReleaseCapture();

                SetCursor((HitTest)m.WParam.ToInt32());

                User32.SendMessage(Handle, (int)WindowsMessages.WM_NCLBUTTONDOWN, m.WParam, (IntPtr)0);
            }

            if (m.Msg == (int)DefMessages.WM_CEF_EDGE_MOVE && Resizable && WindowState == FormWindowState.Normal)
            {
                SetCursor((HitTest)m.WParam.ToInt32());
            }


            base.DefWndProc(ref m);
        }
        #endregion

        #region Public

        public void GoBack()
        {
            this.browser.GoBack();
        }

        public void GoForward()
        {
            this.browser.GoForward();
        }

        public void LoadUrl(string url)
        {
            this.browser.LoadUrl(url);
        }

        public void LoadString(string stringVal, string url)
        {
            this.LoadString(stringVal, url);
        }

        public void LoadString(string stringVal)
        {
            this.LoadString(stringVal);
        }

        #endregion

    }
}
