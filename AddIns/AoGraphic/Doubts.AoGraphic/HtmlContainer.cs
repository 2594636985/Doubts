using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using Doubts.AoGraphic.Dom;
using Doubts.AoGraphic.Entities;
using Doubts.AoGraphic.Handlers;
using Doubts.AoGraphic.Parse;
using Doubts.AoGraphic.Utils;

namespace Doubts.AoGraphic
{
    /// <summary>
    /// HTML容器类
    /// </summary>
    public sealed class HtmlContainer : IDisposable
    {
        #region Fields and Consts

        /// <summary>
        /// CSS树的根
        /// </summary>
        private CssBox _root;

        /// <summary>
        /// dictionary of all css boxes that have ":hover" selector on them
        /// </summary>
        private List<Tupler<CssBox, CssBlock>> _hoverBoxes;

        /// <summary>
        /// Handler for text selection in the html. 
        /// </summary>
        private SelectionHandler _selectionHandler;

        /// <summary>
        /// the text fore color use for selected text
        /// </summary>
        private Color _selectionForeColor;

        /// <summary>
        /// the backcolor to use for selected text
        /// </summary>
        private Color _selectionBackColor;

        /// <summary>
        /// the parsed stylesheet data used for handling the html
        /// </summary>
        private CssData _cssData;

        /// <summary>
        /// Is content selection is enabled for the rendered html (default - true).<br/>
        /// If set to 'false' the rendered html will be static only with ability to click on links.
        /// </summary>
        private bool _isSelectionEnabled = true;

        /// <summary>
        /// Is the build-in context menu enabled (default - true)
        /// </summary>
        private bool _isContextMenuEnabled = true;

        /// <summary>
        /// Gets or sets a value indicating if antialiasing should be avoided 
        /// for geometry like backgrounds and borders
        /// </summary>
        private bool _avoidGeometryAntialias;

        /// <summary>
        /// Gets or sets a value indicating if image asynchronous loading should be avoided (default - false).<br/>
        /// </summary>
        private bool _avoidAsyncImagesLoading;

        /// <summary>
        /// Gets or sets a value indicating if image loading only when visible should be avoided (default - false).<br/>
        /// </summary>
        private bool _avoidImagesLateLoading;

        /// <summary>
        /// Use GDI+ text rendering to measure/draw text.
        /// </summary>
        private bool _useGdiPlusTextRendering;

        /// <summary>
        /// the top-left most location of the rendered html
        /// </summary>
        private PointF _location;

        /// <summary>
        /// the max width and height of the rendered html, effects layout, actual size cannot exceed this values.<br/>
        /// Set zero for unlimited.<br/>
        /// </summary>
        private SizeF _maxSize;

        /// <summary>
        /// Gets or sets the scroll offset of the document for scroll controls
        /// </summary>
        private PointF _scrollOffset;

        /// <summary>
        /// The actual size of the rendered html (after layout)
        /// </summary>
        private SizeF _actualSize;

        #endregion


        /// <summary>
        /// Raised when the user clicks on a link in the html.<br/>
        /// Allows canceling the execution of the link.
        /// </summary>
        public event EventHandler<HtmlLinkClickedEventArgs> LinkClicked;

        /// <summary>
        /// Raised when html renderer requires refresh of the control hosting (invalidation and re-layout).
        /// </summary>
        /// <remarks>
        /// There is no guarantee that the event will be raised on the main thread, it can be raised on thread-pool thread.
        /// </remarks>
        public event EventHandler<HtmlRefreshEventArgs> Refresh;

        /// <summary>
        /// Raised when Html Renderer request scroll to specific location.<br/>
        /// This can occur on document anchor click.
        /// </summary>
        public event EventHandler<HtmlScrollEventArgs> ScrollChange;

        /// <summary>
        /// Raised when an error occurred during html rendering.<br/>
        /// </summary>
        /// <remarks>
        /// There is no guarantee that the event will be raised on the main thread, it can be raised on thread-pool thread.
        /// </remarks>
        public event EventHandler<HtmlRenderErrorEventArgs> RenderError;

        /// <summary>
        /// Raised when a stylesheet is about to be loaded by file path or URI by link element.<br/>
        /// This event allows to provide the stylesheet manually or provide new source (file or Uri) to load from.<br/>
        /// If no alternative data is provided the original source will be used.<br/>
        /// </summary>
        public event EventHandler<HtmlStylesheetLoadEventArgs> StylesheetLoad;

        /// <summary>
        /// Raised when an image is about to be loaded by file path or URI.<br/>
        /// This event allows to provide the image manually, if not handled the image will be loaded from file or download from URI.
        /// </summary>
        public event EventHandler<HtmlImageLoadEventArgs> ImageLoad;

        /// <summary>
        /// the parsed stylesheet data used for handling the html
        /// </summary>
        public CssData CssData
        {
            get { return _cssData; }
        }

        /// <summary>
        /// Gets or sets a value indicating if anti-aliasing should be avoided for geometry like backgrounds and borders (default - false).
        /// </summary>
        public bool AvoidGeometryAntialias
        {
            get { return _avoidGeometryAntialias; }
            set { _avoidGeometryAntialias = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating if image asynchronous loading should be avoided (default - false).<br/>
        /// True - images are loaded synchronously during html parsing.<br/>
        /// False - images are loaded asynchronously to html parsing when downloaded from URL or loaded from disk.<br/>
        /// </summary>
        /// <remarks>
        /// Asynchronously image loading allows to unblock html rendering while image is downloaded or loaded from disk using IO 
        /// ports to achieve better performance.<br/>
        /// Asynchronously image loading should be avoided when the full html content must be available during render, like render to image.
        /// </remarks>
        public bool AvoidAsyncImagesLoading
        {
            get { return _avoidAsyncImagesLoading; }
            set { _avoidAsyncImagesLoading = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating if image loading only when visible should be avoided (default - false).<br/>
        /// True - images are loaded as soon as the html is parsed.<br/>
        /// False - images that are not visible because of scroll location are not loaded until they are scrolled to.
        /// </summary>
        /// <remarks>
        /// Images late loading improve performance if the page contains image outside the visible scroll area, especially if there is large 
        /// amount of images, as all image loading is delayed (downloading and loading into memory).<br/>
        /// Late image loading may effect the layout and actual size as image without set size will not have actual size until they are loaded
        /// resulting in layout change during user scroll.<br/>
        /// Early image loading may also effect the layout if image without known size above the current scroll location are loaded as they
        /// will push the html elements down.
        /// </remarks>
        public bool AvoidImagesLateLoading
        {
            get { return _avoidImagesLateLoading; }
            set { _avoidImagesLateLoading = value; }
        }

        /// <summary>
        /// Use GDI+ text rendering to measure/draw text.<br/>
        /// </summary>
        /// <remarks>
        /// <para>
        /// GDI+ text rendering is less smooth than GDI text rendering but it natively supports alpha channel
        /// thus allows creating transparent images.
        /// </para>
        /// <para>
        /// While using GDI+ text rendering you can control the text rendering using <see cref="Graphics.TextRenderingHint"/>, note that
        /// using <see cref="TextRenderingHint.ClearTypeGridFit"/> doesn't work well with transparent background.
        /// </para>
        /// </remarks>
        public bool UseGdiPlusTextRendering
        {
            get { return _useGdiPlusTextRendering; }
            set
            {
                if (_useGdiPlusTextRendering != value)
                {
                    _useGdiPlusTextRendering = value;
                    RequestRefresh(true);
                }
            }
        }

        /// <summary>
        /// Is content selection is enabled for the rendered html (default - true).<br/>
        /// If set to 'false' the rendered html will be static only with ability to click on links.
        /// </summary>
        public bool IsSelectionEnabled
        {
            get { return _isSelectionEnabled; }
            set { _isSelectionEnabled = value; }
        }

        /// <summary>
        /// Is the build-in context menu enabled and will be shown on mouse right click (default - true)
        /// </summary>
        public bool IsContextMenuEnabled
        {
            get { return _isContextMenuEnabled; }
            set { _isContextMenuEnabled = value; }
        }

        /// <summary>
        /// 显示的偏移
        /// </summary>
        public PointF ScrollOffset
        {
            get { return _scrollOffset; }
            set { _scrollOffset = value; }
        }

        /// <summary>
        /// 显示的位置
        /// </summary>
        public PointF Location
        {
            get { return _location; }
            set { _location = value; }
        }

        /// <summary>
        /// HTML容器的最大长宽
        /// </summary>
        public SizeF MaxSize
        {
            get { return _maxSize; }
            set { _maxSize = value; }
        }

        /// <summary>
        /// 实际的长宽
        /// </summary>
        public SizeF ActualSize
        {
            get { return _actualSize; }
            internal set { _actualSize = value; }
        }

        /// <summary>
        /// Get the currently selected text segment in the html.
        /// </summary>
        public string SelectedText
        {
            get { return _selectionHandler.GetSelectedText(); }
        }

        /// <summary>
        /// Copy the currently selected html segment with style.
        /// </summary>
        public string SelectedHtml
        {
            get { return _selectionHandler.GetSelectedHtml(); }
        }

        /// <summary>
        ///  CSS树的根
        /// </summary>
        internal CssBox Root
        {
            get { return _root; }
        }

        /// <summary>
        /// CSS选中的前景色
        /// </summary>
        internal Color SelectionForeColor
        {
            get { return _selectionForeColor; }
            set { _selectionForeColor = value; }
        }

        /// <summary>
        /// CSS选中的背景色
        /// </summary>
        internal Color SelectionBackColor
        {
            get { return _selectionBackColor; }
            set { _selectionBackColor = value; }
        }

        /// <summary>
        /// 设置相关的HTML内容
        /// </summary>
        /// <param name="htmlSource"></param>
        /// <param name="baseCssData"></param>
        public void SetHtml(string htmlSource, CssData baseCssData = null)
        {
            if (_root != null)
            {
                _root.Dispose();
                _root = null;
                if (_selectionHandler != null)
                    _selectionHandler.Dispose();
                _selectionHandler = null;
            }

            if (!string.IsNullOrEmpty(htmlSource))
            {
                _cssData = baseCssData ?? CssUtils.DefaultCssData;

                _root = DomParser.GenerateCssTree(htmlSource, this, ref _cssData);

                if (_root != null)
                {
                    _selectionHandler = new SelectionHandler(_root);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="styleGen"></param>
        /// <returns></returns>
        public string GetHtml(HtmlGenerationStyle styleGen = HtmlGenerationStyle.Inline)
        {
            return DomUtils.GenerateHtml(_root, styleGen);
        }

        /// <summary>
        /// 获得指定位置的相应属性的值
        /// </summary>
        /// <param name="location"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public string GetAttributeAt(Point location, string attribute)
        {
            ArgChecker.AssertArgNotNullOrEmpty(attribute, "attribute");

            var cssBox = DomUtils.GetCssBox(_root, OffsetByScroll(location));
            return cssBox != null ? DomUtils.GetAttribute(cssBox, attribute) : null;
        }

        /// <summary>
        /// 获得指定位置的连接
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public string GetLinkAt(Point location)
        {
            var link = DomUtils.GetLinkBox(_root, OffsetByScroll(location));
            return link != null ? link.HrefLink : null;
        }

        /// <summary>
        /// Get the rectangle of html element as calculated by html layout.<br/>
        /// Element if found by id (id attribute on the html element).<br/>
        /// Note: to get the screen rectangle you need to adjust by the hosting control.<br/>
        /// </summary>
        /// <param name="elementId">the id of the element to get its rectangle</param>
        /// <returns>the rectangle of the element or null if not found</returns>
        public RectangleF? GetElementRectangle(string elementId)
        {
            ArgChecker.AssertArgNotNullOrEmpty(elementId, "elementId");

            var box = DomUtils.GetBoxById(_root, elementId.ToLower());
            return box != null ? CommonUtils.GetFirstValueOrDefault(box.Rectangles, box.Bounds) : (RectangleF?)null;
        }

        /// <summary>
        /// 测量每一个CSS盒子的位置和长宽
        /// </summary>
        /// <param name="g"></param>
        public void PerformLayout(Graphics g)
        {
            ArgChecker.AssertArgNotNull(g, "g");

            if (_root != null)
            {
                //using(var ig = new WinGraphics(g, _useGdiPlusTextRendering
                var ig = new AoGraphics(g);

                _actualSize = SizeF.Empty;

                // if width is not restricted we set it to large value to get the actual later
                _root.Size = new SizeF(_maxSize.Width > 0 ? _maxSize.Width : 99999, 0);
                _root.Location = _location;
                _root.PerformLayout(ig);

                if (_maxSize.Width <= 0.1)
                {
                    // in case the width is not restricted we need to double layout, first will find the width so second can layout by it (center alignment)
                    _root.Size = new SizeF((int)Math.Ceiling(_actualSize.Width), 0);
                    _actualSize = SizeF.Empty;
                    _root.PerformLayout(ig);
                }

            }
        }

        /// <summary>
        /// 绘画HTML
        /// </summary>
        /// <param name="g"></param>
        public void PerformPaint(Graphics g)
        {
            ArgChecker.AssertArgNotNull(g, "g");

            Region prevClip = null;
            if (MaxSize.Height > 0)
            {
                prevClip = g.Clip;
                g.SetClip(new RectangleF(_location, _maxSize));
            }

            if (_root != null)
            {
                //using(var ig = new WinGraphics(g, _useGdiPlusTextRendering))
                using (var ig = new AoGraphics(g))
                {
                    _root.Paint(ig);
                }
            }

            if (prevClip != null)
            {
                g.SetClip(prevClip, CombineMode.Replace);
            }
        }

        /// <summary>
        /// Handle mouse down to handle selection.
        /// </summary>
        /// <param name="parent">the control hosting the html to invalidate</param>
        /// <param name="e">the mouse event args</param>
        public void HandleMouseDown(Control parent, MouseEventArgs e)
        {
            ArgChecker.AssertArgNotNull(parent, "parent");
            ArgChecker.AssertArgNotNull(e, "e");

            try
            {
                if (_selectionHandler != null)
                    _selectionHandler.HandleMouseDown(parent, OffsetByScroll(e.Location), IsMouseInContainer(e.Location));
            }
            catch (Exception ex)
            {
                ReportError(HtmlRenderErrorType.KeyboardMouse, "Failed mouse down handle", ex);
            }
        }

        /// <summary>
        /// Handle mouse up to handle selection and link click.
        /// </summary>
        /// <param name="parent">the control hosting the html to invalidate</param>
        /// <param name="e">the mouse event args</param>
        public void HandleMouseUp(Control parent, MouseEventArgs e)
        {
            ArgChecker.AssertArgNotNull(parent, "parent");
            ArgChecker.AssertArgNotNull(e, "e");

            try
            {
                if (_selectionHandler != null && IsMouseInContainer(e.Location))
                {
                    var ignore = _selectionHandler.HandleMouseUp(parent, e.Button);
                    if (!ignore && (e.Button & MouseButtons.Left) != 0)
                    {
                        var loc = OffsetByScroll(e.Location);
                        var link = DomUtils.GetLinkBox(_root, loc);
                        if (link != null)
                        {
                            HandleLinkClicked(parent, e, link);
                        }
                    }
                }
            }
            catch (HtmlLinkClickedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ReportError(HtmlRenderErrorType.KeyboardMouse, "Failed mouse up handle", ex);
            }
        }

        /// <summary>
        /// Handle mouse double click to select word under the mouse.
        /// </summary>
        /// <param name="parent">the control hosting the html to set cursor and invalidate</param>
        /// <param name="e">mouse event args</param>
        public void HandleMouseDoubleClick(Control parent, MouseEventArgs e)
        {
            ArgChecker.AssertArgNotNull(parent, "parent");
            ArgChecker.AssertArgNotNull(e, "e");

            try
            {
                if (_selectionHandler != null && IsMouseInContainer(e.Location))
                    _selectionHandler.SelectWord(parent, OffsetByScroll(e.Location));
            }
            catch (Exception ex)
            {
                ReportError(HtmlRenderErrorType.KeyboardMouse, "Failed mouse double click handle", ex);
            }
        }

        /// <summary>
        /// Handle mouse move to handle hover cursor and text selection.
        /// </summary>
        /// <param name="parent">the control hosting the html to set cursor and invalidate</param>
        /// <param name="e">the mouse event args</param>
        public void HandleMouseMove(Control parent, MouseEventArgs e)
        {
            ArgChecker.AssertArgNotNull(parent, "parent");
            ArgChecker.AssertArgNotNull(e, "e");

            try
            {
                var loc = OffsetByScroll(e.Location);
                if (_selectionHandler != null && IsMouseInContainer(e.Location))
                    _selectionHandler.HandleMouseMove(parent, loc);

                /*
                if( _hoverBoxes != null )
                {
                    bool refresh = false;
                    foreach(var hoverBox in _hoverBoxes)
                    {
                        foreach(var rect in hoverBox.Item1.Rectangles.Values)
                        {
                            if( rect.Contains(loc) )
                            {
                                //hoverBox.Item1.Color = "gold";
                                refresh = true;
                            }
                        }
                    }

                    if(refresh)
                        RequestRefresh(true);
                }
                 */
            }
            catch (Exception ex)
            {
                ReportError(HtmlRenderErrorType.KeyboardMouse, "Failed mouse move handle", ex);
            }
        }

        /// <summary>
        /// Handle mouse leave to handle hover cursor.
        /// </summary>
        /// <param name="parent">the control hosting the html to set cursor and invalidate</param>
        public void HandleMouseLeave(Control parent)
        {
            ArgChecker.AssertArgNotNull(parent, "parent");

            try
            {
                if (_selectionHandler != null)
                    _selectionHandler.HandleMouseLeave(parent);
            }
            catch (Exception ex)
            {
                ReportError(HtmlRenderErrorType.KeyboardMouse, "Failed mouse leave handle", ex);
            }
        }

        /// <summary>
        /// Handle key down event for selection and copy.
        /// </summary>
        /// <param name="parent">the control hosting the html to invalidate</param>
        /// <param name="e">the pressed key</param>
        public void HandleKeyDown(Control parent, KeyEventArgs e)
        {
            ArgChecker.AssertArgNotNull(parent, "parent");
            ArgChecker.AssertArgNotNull(e, "e");

            try
            {
                if (e.Control && _selectionHandler != null)
                {
                    // select all
                    if (e.KeyCode == Keys.A)
                    {
                        _selectionHandler.SelectAll(parent);
                    }

                    // copy currently selected text
                    if (e.KeyCode == Keys.C)
                    {
                        _selectionHandler.CopySelectedHtml();
                    }
                }
            }
            catch (Exception ex)
            {
                ReportError(HtmlRenderErrorType.KeyboardMouse, "Failed key down handle", ex);
            }
        }

        /// <summary>
        /// Raise the stylesheet load event with the given event args.
        /// </summary>
        /// <param name="args">the event args</param>
        internal void RaiseHtmlStylesheetLoadEvent(HtmlStylesheetLoadEventArgs args)
        {
            try
            {
                if (StylesheetLoad != null)
                {
                    StylesheetLoad(this, args);
                }
            }
            catch (Exception ex)
            {
                ReportError(HtmlRenderErrorType.CssParsing, "Failed stylesheet load event", ex);
            }
        }

        /// <summary>
        /// Raise the image load event with the given event args.
        /// </summary>
        /// <param name="args">the event args</param>
        internal void RaiseHtmlImageLoadEvent(HtmlImageLoadEventArgs args)
        {
            try
            {
                if (ImageLoad != null)
                {
                    ImageLoad(this, args);
                }
            }
            catch (Exception ex)
            {
                ReportError(HtmlRenderErrorType.Image, "Failed image load event", ex);
            }
        }

        /// <summary>
        /// Request invalidation and re-layout of the control hosting the renderer.
        /// </summary>
        /// <param name="layout">is re-layout is required for the refresh</param>
        internal void RequestRefresh(bool layout)
        {
            try
            {
                if (Refresh != null)
                {
                    Refresh(this, new HtmlRefreshEventArgs(layout));
                }
            }
            catch (Exception ex)
            {
                ReportError(HtmlRenderErrorType.General, "Failed refresh request", ex);
            }
        }

        /// <summary>
        /// Report error in html render process.
        /// </summary>
        /// <param name="type">the type of error to report</param>
        /// <param name="message">the error message</param>
        /// <param name="exception">optional: the exception that occured</param>
        internal void ReportError(HtmlRenderErrorType type, string message, Exception exception = null)
        {
            try
            {
                if (RenderError != null)
                {
                    RenderError(this, new HtmlRenderErrorEventArgs(type, message, exception));
                }
            }
            catch
            { }
        }

        /// <summary>
        /// Handle link clicked going over <see cref="LinkClicked"/> event and using <see cref="Process.Start()"/> if not canceled.
        /// </summary>
        /// <param name="parent">the control hosting the html to invalidate</param>
        /// <param name="e">the mouse event args</param>
        /// <param name="link">the link that was clicked</param>
        internal void HandleLinkClicked(Control parent, MouseEventArgs e, CssBox link)
        {
            if (LinkClicked != null)
            {
                var args = new HtmlLinkClickedEventArgs(link.HrefLink, link.HtmlTag.Attributes);
                try
                {
                    LinkClicked(this, args);
                }
                catch (Exception ex)
                {
                    throw new HtmlLinkClickedException("Error in link clicked intercept", ex);
                }
                if (args.Handled)
                    return;
            }

            if (!string.IsNullOrEmpty(link.HrefLink))
            {
                if (link.HrefLink.StartsWith("#") && link.HrefLink.Length > 1)
                {
                    if (ScrollChange != null)
                    {
                        var rect = GetElementRectangle(link.HrefLink.Substring(1));
                        if (rect.HasValue)
                        {
                            ScrollChange(this, new HtmlScrollEventArgs(Point.Round(rect.Value.Location)));
                            HandleMouseMove(parent, e);
                        }
                    }
                }
                else
                {
                    var nfo = new ProcessStartInfo(link.HrefLink);
                    nfo.UseShellExecute = true;
                    Process.Start(nfo);
                }
            }
        }

        /// <summary>
        /// Add css box that has ":hover" selector to be handled on mouse hover.
        /// </summary>
        /// <param name="box">the box that has the hover selector</param>
        /// <param name="block">the css block with the css data with the selector</param>
        internal void AddHoverBox(CssBox box, CssBlock block)
        {
            ArgChecker.AssertArgNotNull(box, "box");
            ArgChecker.AssertArgNotNull(block, "block");

            if (_hoverBoxes == null)
                _hoverBoxes = new List<Tupler<CssBox, CssBlock>>();

            _hoverBoxes.Add(new Tupler<CssBox, CssBlock>(box, block));
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
        }


        #region Private methods

        /// <summary>
        /// Adjust the offset of the given location by the current scroll offset.
        /// </summary>
        /// <param name="location">the location to adjust</param>
        /// <returns>the adjusted location</returns>
        private Point OffsetByScroll(Point location)
        {
            location.Offset(-(int)ScrollOffset.X, -(int)ScrollOffset.Y);
            return location;
        }

        /// <summary>
        /// Check if the mouse is currently on the html container.<br/>
        /// Relevant if the html container is not filled in the hosted control (location is not zero and the size is not the full size of the control).
        /// </summary>
        private bool IsMouseInContainer(Point location)
        {
            return location.X >= _location.X && location.X <= _location.X + _actualSize.Width && location.Y >= _location.Y + ScrollOffset.Y && location.Y <= _location.Y + ScrollOffset.Y + _actualSize.Height;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        private void Dispose(bool all)
        {
            try
            {
                if (all)
                {
                    LinkClicked = null;
                    Refresh = null;
                    RenderError = null;
                    StylesheetLoad = null;
                    ImageLoad = null;
                }

                _cssData = null;
                if (_root != null)
                    _root.Dispose();
                _root = null;
                if (_selectionHandler != null)
                    _selectionHandler.Dispose();
                _selectionHandler = null;
            }
            catch
            { }
        }

        #endregion
    }
}