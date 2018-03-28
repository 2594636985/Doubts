using System;
using System.Collections.Generic;
using Hydra.AomiCss.Entities;
using Hydra.AomiCss.Parse;
using Hydra.AomiCss.Utils;

namespace Hydra.AomiCss
{
    /// <summary>
    /// CSS数据类
    /// </summary>
    public sealed class CssData
    {
        private static readonly List<CssBlock> _emptyArray = new List<CssBlock>();
        private readonly Dictionary<string, Dictionary<string, List<CssBlock>>> _mediaBlocks = new Dictionary<string, Dictionary<string, List<CssBlock>>>(StringComparer.InvariantCultureIgnoreCase);


        internal CssData()
        {
            _mediaBlocks.Add("all", new Dictionary<string, List<CssBlock>>(StringComparer.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// 解析CSS样式，如果combineWithDefault为值的时候，以默认的
        /// </summary>
        /// <param name="stylesheet"></param>
        /// <param name="combineWithDefault"></param>
        /// <returns></returns>
        public static CssData Parse(string stylesheet, bool combineWithDefault = true)
        {
            return CssParser.ParseStyleSheet(stylesheet, combineWithDefault);
        }


        /// <summary>
        /// 获得所有媒介
        /// </summary>
        internal IDictionary<string, Dictionary<string, List<CssBlock>>> MediaBlocks
        {
            get { return _mediaBlocks; }
        }

        /// <summary>
        /// Check if there are css blocks for the given class selector.
        /// </summary>
        /// <param name="className">the class selector to check for css blocks by</param>
        /// <param name="media">optinal: the css media type (default - all)</param>
        /// <returns>true - has css blocks for the class, false - otherwise</returns>
        public bool ContainsCssBlock(string className, string media = "all")
        {
            Dictionary<string, List<CssBlock>> mid;

            return _mediaBlocks.TryGetValue(media, out mid) && mid.ContainsKey(className);
        }

        /// <summary>
        /// Get collection of css blocks for the requested class selector.<br/>
        /// the <paramref name="className"/> can be: class name, html element name, html element and 
        /// class name (elm.class), hash tag with element id (#id).<br/>
        /// returned all the blocks that word on the requested class selector, it can contain simple
        /// selector or hierarchy selector.
        /// </summary>
        /// <param name="className">the class selector to get css blocks by</param>
        /// <param name="media">optinal: the css media type (default - all)</param>
        /// <returns>collection of css blocks, empty collection if no blocks exists (never null)</returns>
        public IEnumerable<CssBlock> GetCssBlock(string className, string media = "all")
        {
            List<CssBlock> block = null;
            Dictionary<string, List<CssBlock>> mid;
            if (_mediaBlocks.TryGetValue(media, out mid))
            {
                mid.TryGetValue(className, out block);
            }
            return block ?? _emptyArray;
        }

        /// <summary>
        /// Add the given css block to the css data, merging to existing block if required.
        /// </summary>
        /// <remarks>
        /// If there is no css blocks for the same class it will be added to data collection.<br/>
        /// If there is already css blocks for the same class it will check for each existing block
        /// if the hierarchical selectors match (or not exists). if do the two css blocks will be merged into
        /// one where the new block properties overwrite existing if needed. if the new block doesn't mach any
        /// existing it will be added either to the beggining of the list if it has no  hierarchical selectors or at the end.<br/>
        /// Css block without hierarchical selectors must be added to the beginning of the list so more specific block
        /// can overwrite it when the style is applied.
        /// </remarks>
        /// <param name="media">the media type to add the CSS to</param>
        /// <param name="cssBlock">the css block to add</param>
        public void AddCssBlock(string media, CssBlock cssBlock)
        {
            Dictionary<string, List<CssBlock>> mid;
            if (!_mediaBlocks.TryGetValue(media, out mid))
            {
                mid = new Dictionary<string, List<CssBlock>>(StringComparer.InvariantCultureIgnoreCase);
                _mediaBlocks.Add(media, mid);
            }

            if (!mid.ContainsKey(cssBlock.Class))
            {
                var list = new List<CssBlock>();
                list.Add(cssBlock);
                mid[cssBlock.Class] = list;
            }
            else
            {
                bool merged = false;
                var list = mid[cssBlock.Class];
                foreach (var block in list)
                {
                    if (block.EqualsSelector(cssBlock))
                    {
                        merged = true;
                        block.Merge(cssBlock);
                        break;
                    }
                }

                if (!merged)
                {
                    // general block must be first
                    if (cssBlock.Selectors == null)
                        list.Insert(0, cssBlock);
                    else
                        list.Add(cssBlock);
                }
            }
        }

        /// <summary>
        /// Combine this CSS data blocks with <paramref name="other"/> CSS blocks for each media.<br/>
        /// Merge blocks if exists in both.
        /// </summary>
        /// <param name="other">the CSS data to combine with</param>
        public void Combine(CssData other)
        {
            ArgChecker.AssertArgNotNull(other, "other");

            // for each media block
            foreach (var mediaBlock in other.MediaBlocks)
            {
                // for each css class in the media block
                foreach (var bla in mediaBlock.Value)
                {
                    // for each css block of the css class
                    foreach (var cssBlock in bla.Value)
                    {
                        // combine with this
                        AddCssBlock(mediaBlock.Key, cssBlock);
                    }
                }
            }
        }

        /// <summary>
        /// Create deep copy of the css data with cloned css blocks.
        /// </summary>
        /// <returns>cloned object</returns>
        public CssData Clone()
        {
            var clone = new CssData();
            foreach (var mid in _mediaBlocks)
            {
                var cloneMid = new Dictionary<string, List<CssBlock>>(StringComparer.InvariantCultureIgnoreCase);
                foreach (var blocks in mid.Value)
                {
                    var cloneList = new List<CssBlock>();
                    foreach (var cssBlock in blocks.Value)
                    {
                        cloneList.Add(cssBlock.Clone());
                    }
                    cloneMid[blocks.Key] = cloneList;
                }
                clone._mediaBlocks[mid.Key] = cloneMid;
            }
            return clone;
        }
    }
}
