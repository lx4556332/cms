//
// HTML创建
//  修改说明：
//      2012-12-29  newmin  [+]:Link & SAList
//      2013-03-05  newmin  [+]: 标签缩写
//      2013-03-06  newmin  [+]: 评论模块
//      2013-03-07  newmin  [+]: 添加数据标签参数符 "[ ]",主要用于outline[200]
//      2013-03-11  newmin  [+]: 添加authorname列，用于显示作者名称
//  2013-04-25  22:28 newmin [+]:PagerArchiveList添加
//  2013-06-07  22:15 newmin [+]:MCategoryTree,MCategoryList
//  2013-06-08  10:02 newmin [!]:CategoryTree_Iterator 加入TreeResultHandle,并加入isRoot,判断root类是否模块相同
//  2013-06-08  10:22 newmin [-]:删除MCategoryTree
//  2013-09-05  07:14 newmin [+]:添加region
//
using Ops.Cms.Cache.CacheCompoment;
using Ops.Cms.Domain.Interface.Enum;

namespace Ops.Cms.Template
{
    using Ops.Cms;
    using Ops.Cms.Cache;
    using Ops.Cms.CacheService;
    using Ops.Cms.DataTransfer;
    using Ops.Cms.Domain;
    using Ops.Cms.Domain.Interface.Site.Category;
    using Ops.Framework.Xml;
    using Ops.Regions;
    using Ops.Template;
    using Spc.Models;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using model = Spc.Models;

    [XmlObject("CmsTemplateTag", "模板标签")]
    public partial class CmsTemplates : CmsTemplateDataMethod
    {
        private static Type __type;

        //标签文件
        static CmsTemplates()
        {
            __type = typeof(CmsTemplates);
        }


        #region 文档

        /* ==================================== 查看文档 ====================================*/

        /// <summary>
        /// 文档内容
        /// </summary>
        /// <param name="idOrAlias"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        [TemplateTag]
        [XmlObjectProperty("获取文档", @"
        	<b>参数：</b><br />
        	==========================<br />
			1:文档的编号或别名（可以文档列表中查询）
			2:HTML格式
		")]
        public string Archive(string idOrAlias, string format)
        {
            object id;
            if (Regex.IsMatch(idOrAlias, "^\\d+$"))
            {
                id = int.Parse(idOrAlias);
            }
            else
            {
                id = idOrAlias;
            }
            return base.Archive(this.site.SiteId, id, format);
        }

        [TemplateTag]
        [XmlObjectProperty("获取文档", @"
        	<b>参数：</b><br />
        	==========================<br />
			1:HTML格式
		")]
        public string Archive(string format)
        {
            object id = HttpContext.Current.Items["archive.id"];
            if (id== null || id == String.Empty)
            {
                return this.TplMessage("Error: 此标签只能在文档页面中调用!");
            }
            return base.Archive(this.site.SiteId, id, format);
        }


        [TemplateTag]
        [XmlObjectProperty("获取上一篇文档", @"
        	<b>参数：</b><br />
        	==========================<br />
			1:HTML格式
		")]
        public string PrevArchive(string format)
        {
            object id = HttpContext.Current.Items["archive.id"];
            if (id == null)
            {
                return this.TplMessage("Error: 此标签只能在文档页面中调用!");
            }
            return PrevArchive(id.ToString(), format);
        }

        [TemplateTag]
        [XmlObjectProperty("获取下一篇文档", @"
        	<b>参数：</b><br />
        	==========================<br />
			1:HTML格式
		")]
        public string NextArchive(string format)
        {
            object id = HttpContext.Current.Items["archive.id"];
            if (id == null)
            {
                return this.TplMessage("Error: 此标签只能在文档页面中调用!");
            }
            return NextArchive(id.ToString(), format);
        }


        [TemplateTag]
        [ContainSetting]
        [XmlObjectProperty("获取文档(默认格式)", @"")]
        public string Archive()
        {
            return this.Archive(base.TplSetting.CFG_ArchiveFormat);
        }

        [TemplateTag]
        [XmlObjectProperty("获取上一篇文档(默认格式)", @"")]
        public string PrevArchive()
        {
            return PrevArchive(base.TplSetting.CFG_PrevArchiveFormat);
        }

        [TemplateTag]
        [XmlObjectProperty("获取下一篇文档(默认格式)", @"")]
        public string NextArchive()
        {
            return NextArchive(base.TplSetting.CFG_NextArchiveFormat);
        }


        #endregion

        #region 文档列表

        //====================== 普通列表 ==========================//

        /// <summary>
        /// 文档列表
        /// </summary>
        [XmlObjectProperty("获取栏目下的文档列表。", @"
        	<b>参数：</b><br />
        	==========================<br />
        	1：栏目Tag<br />
        	2：数目<br />
			3：HTML格式<br />
			4：是否包含子栏目,可选值[0|1]
		")]
        private string Archives(string param, string num, string format, bool container)
        {
            int _num;
            IEnumerable<ArchiveDto> archives = null;
            CategoryDto category = default(CategoryDto);
            int.TryParse(num, out _num);

            //栏目
            category = ServiceCall.Instance.SiteService.GetCategory(this.siteId, param);

            if (!(category.Id > 0))
            {
                return String.Format("ERROR:模块或栏目不存在!参数:{0}", param);
            }
            archives = container ? ServiceCall.Instance.ArchiveService.GetArchivesContainChildCategories(this.siteId, category.Lft, category.Rgt, _num) :
                ServiceCall.Instance.ArchiveService.GetArchivesByCategoryTag(this.siteId, category.Tag, _num);

            return this.ArchiveList(archives, format);
        }

        /// <summary>
        /// 文档列表(包含子类)
        /// </summary>
        /// <param name="categoryTag"></param>
        /// <param name="num"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        [TemplateTag]
        [XmlObjectProperty("获取栏目（包含子栏目）下的文档列表。", @"
        	<b>参数：</b><br />
        	==========================<br />
        	1：栏目Tag<br />
        	2：数目<br />
			3：HTML格式
		")]
        public string Archives(string param, string num, string format)
        {
            if (Regex.IsMatch(param, "^\\d+$"))
            {
                int _num;
                IEnumerable<ArchiveDto> dt = null;
                model.Module module = null;
                int.TryParse(num, out _num);

                module = CmsLogic.Module.GetModule(int.Parse(param));
                if (module != null)
                {
                    dt = ServiceCall.Instance.ArchiveService.GetArchivesByModuleId(this.siteId, module.ID, _num);
                    return ArchiveList(dt, format);
                }
            }

            return this.Archives(param, num, format, true);
        }


        /// <summary>
        /// 文档列表(包含子类)
        /// </summary>
        /// <param name="container"></param>
        /// <param name="num"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        [TemplateTag]
        [XmlObjectProperty("获取栏目（包含子栏目）下的文档列表。", @"
        	<b>参数：</b><br />
        	==========================<br />
        	1：数目<br />
			2：HTML格式
		")]
        public string Archives(string num, string format)
        {
            string tag = HttpContext.Current.Items["category.tag"] as string;
            if (String.IsNullOrEmpty(tag))
            {
                return this.TplMessage("Error: 此标签不允许在当前页面中调用!");
            }
            return Archives(tag, num, format, true);
        }



        /// <summary>
        /// 文档列表(包含子类)
        /// </summary>
        /// <param name="num"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        [TemplateTag]
        [ContainSetting]
        [XmlObjectProperty("获取栏目（包含子栏目）下的文档列表。", @"
        	<b>参数：</b><br />
        	==========================<br />
        	1：数目
		")]
        public string Archives(string num)
        {
            return Archives(num, base.TplSetting.CFG_ArchiveLinkFormat);
        }

        /// <summary>
        /// 文档列表(不包含子类)
        /// </summary>
        /// <param name="categoryTag"></param>
        /// <param name="num"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        [TemplateTag]
        [XmlObjectProperty("获取栏目（不含子栏目）下的文档列表。", @"
        	<b>参数：</b><br />
        	==========================<br />
        	1：栏目Tag<br />
        	2：数目<br />
			3：HTML格式
		")]
        public string Archives2(string categoryTag, string num, string format)
        {
            return this.Archives(categoryTag, num, format, false);
        }

        /// <summary>
        /// 文档列表(不包含子类)
        /// </summary>
        /// <param name="container"></param>
        /// <param name="num"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        [TemplateTag]
        [XmlObjectProperty("获取栏目（不含子栏目）下的文档列表。", @"
        	<b>参数：</b><br />
        	==========================<br />
        	1：数目<br />
			2：HTML格式
		")]
        public string Archives2(string num, string format)
        {
            string tag = HttpContext.Current.Items["category.tag"] as string;
            if (String.IsNullOrEmpty(tag))
            {
                return this.TplMessage("Error: 此标签不允许在当前页面中调用!");
            }
            return Archives2(tag, num, format);
        }

        /// <summary>
        /// 文档列表(不包含子类)
        /// </summary>
        /// <param name="num"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        [TemplateTag]
        [ContainSetting]
        [XmlObjectProperty("获取栏目（不含子栏目）下的文档列表。", @"
        	<b>参数：</b><br />
        	==========================<br />
        	1：数目
		")]
        public string Archives2(string num)
        {
            return Archives2(num, base.TplSetting.CFG_ArchiveLinkFormat);
        }



        //====================== 特殊文档列表 ==========================//

        /// <summary>
        /// 特殊文档
        /// </summary>
        /// <param name="tag">栏目标签</param>
        /// <param name="container">是否包括子类</param>
        /// <param name="num">数量</param>
        /// <param name="format">格式
        /// </param>
        /// <returns></returns>
        [TemplateTag]
        [XmlObjectProperty("获取标记为【推荐】的文档列表", @"
        	<b>参数：</b><br />
        	==========================<br />
        	1：栏目编号<br />
        	2：数目<br />
			3：HTML格式<br />
			4：是否包含子栏目，可选值为[0,1],1代表包含。
		")]
        public string SpecialArchives(string param, string num, string format, bool container)
        {
            int _num;
            IEnumerable<ArchiveDto> dt = null;
            CategoryDto category = default(CategoryDto);
            model.Module module = null;
            int.TryParse(num, out _num);

            //如果传入为模块编号
            if (Regex.IsMatch(param, "^\\d+$"))
            {
                module = CmsLogic.Module.GetModule(int.Parse(param));
                if (module != null)
                {
                    dt = ServiceCall.Instance.ArchiveService.GetSpecialArchivesByModuleId(this.siteId, module.ID, _num);
                }
            }

            if (module == null)
            {
                //获取栏目
                category = ServiceCall.Instance.SiteService.GetCategory(this.siteId, param);

                if (!(category.Id > 0))
                {
                    return String.Format("ERROR:模块或栏目不存在!参数:{0}", param);
                }

                dt = container ?
                    ServiceCall.Instance.ArchiveService.GetSpecialArchives(this.siteId, category.Lft, category.Rgt, _num) :
                    ServiceCall.Instance.ArchiveService.GetSpecialArchives(this.siteId, category.Tag, _num);
            }


            return this.ArchiveList(dt, format);
        }


        /// <summary>
        /// 特殊文档(包含子栏目)
        /// </summary>
        /// <param name="categoryTag"></param>
        /// <param name="num"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        [TemplateTag]
        [XmlObjectProperty("获取标记为【推荐】的文档列表,默认包含子栏目", @"
        	<b>参数：</b><br />
        	==========================<br />
        	1：栏目编号<br />
        	2：数目<br />
			3：HTML格式
		")]
        public string SpecialArchives(string param, string num, string format)
        {
            return this.SpecialArchives(param, num, format, true);
        }

        /// <summary>
        /// 特殊文档(包含子栏目)
        /// </summary>
        /// <param name="container"></param>
        /// <param name="num"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        [TemplateTag]
        [XmlObjectProperty("获取标记为【推荐】的文档列表,默认包含子栏目", @"
        	<p class=""red"">仅能在栏目页中（默认文件：category.html) 使用此标签</p>
        	
        	<b>参数：</b><br />
        	==========================<br />
        	1：数目<br />
			2：HTML格式
		")]
        public string SpecialArchives(string num, string format)
        {
            string id = HttpContext.Current.Items["category.tag"] as string;
            if (String.IsNullOrEmpty(id))
            {
                return this.TplMessage("Error: 此标签不允许在当前页面中调用!");
            }
            return SpecialArchives(id, num, format, true);
        }

        /// <summary>
        /// 特殊文档(包含子栏目)
        /// </summary>
        /// <param name="num"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        [TemplateTag]
        [ContainSetting]
        [XmlObjectProperty("获取标记为【推荐】的文档列表,默认包含子栏目,Html格式使用后台设置的格式。", @"
        	<p class=""red"">仅能在栏目页中（默认文件：category.html) 使用此标签</p>
        	<b>参数：</b><br />
        	==========================<br />
        	1：数目
		")]
        public string SpecialArchives(string num)
        {
            return SpecialArchives(num, base.TplSetting.CFG_ArchiveLinkFormat);
        }

        /// <summary>
        /// 特殊文档(包含子栏目)
        /// </summary>
        /// <param name="categoryTag"></param>
        /// <param name="num"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        [TemplateTag]
        [XmlObjectProperty("获取标记为【推荐】的文档列表,默认包含子栏目,Html格式使用后台设置的格式。", @"
        	<p class=""red"">仅能在栏目页中（默认文件：category.html) 使用此标签</p>
        	<b>参数：</b><br />
        	==========================<br />
        	1：栏目Tag<br />
        	2：显示数量<br />
        	3：HTML格式
		")]
        public string SpecialArchives2(string param, string num, string format)
        {
            return this.SpecialArchives(param, num, format, false);
        }


        /// <summary>
        /// 特殊文档(包含子栏目)
        /// </summary>
        /// <param name="container"></param>
        /// <param name="num"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        [TemplateTag]
        [XmlObjectProperty("获取标记为【推荐】的文档列表,默认包含子栏目,Html格式使用后台设置的格式。", @"
        	<p class=""red"">仅能在栏目页中（默认文件：category.html) 使用此标签</p>
        	<b>参数：</b><br />
        	==========================<br />
        	1：显示数量<br />
        	2：HTML格式
		")]
        public string SpecialArchives2(string num, string format)
        {
            string id = HttpContext.Current.Items["category.tag"] as string;
            if (String.IsNullOrEmpty(id))
            {
                return this.TplMessage("Error: 此标签不允许在当前页面中调用!");
            }
            return SpecialArchives(id, num, format, false);
        }

        /// <summary>
        /// 特殊文档(包含子栏目)
        /// </summary>
        /// <param name="num"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        [TemplateTag]
        [XmlObjectProperty("获取标记为【推荐】的文档列表,默认包含子栏目,Html格式使用后台设置的格式。", @"
        	<p class=""red"">仅能在栏目页中（默认文件：category.html) 使用此标签</p>
        	<b>参数：</b><br />
        	==========================<br />
        	1：显示数量
		")]
        public string SpecialArchives2(string num)
        {
            return SpecialArchives2(num, base.TplSetting.CFG_ArchiveLinkFormat);
        }


        //====================== 浏览排行列表 ==========================//

        /// <summary>
        /// 按点击排行文档列表
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="container"></param>
        /// <param name="num"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        [TemplateTag]
        [XmlObjectProperty("获取热门文档列表", @"
        	<b>参数：</b><br />
        	==========================<br />
        	1：栏目Tag<br />
        	2：显示数量<br />
        	3：HTML格式<br />
        	4：是否包含子栏目
		")]
        public string HotArchives(string categoryTag, string num, string format, bool container)
        {
            int _num;
            IEnumerable<ArchiveDto> dt = null;
            CategoryDto category = default(CategoryDto);
            int.TryParse(num, out _num);

            category = ServiceCall.Instance.SiteService.GetCategory(this.siteId, categoryTag);
            if (!(category.Id > 0))
            {
                return String.Format("ERROR:模块或栏目不存在!参数:{0}", categoryTag);
            }

            dt = container ?
                ServiceCall.Instance.ArchiveService.GetArchivesByViewCount(this.siteId, category.Lft, category.Rgt, _num) :
                ServiceCall.Instance.ArchiveService.GetArchivesByViewCount(this.siteId, category.Tag, _num);


            return this.ArchiveList(dt, format);
        }

        /// <summary>
        /// 按点击排行文档列表
        /// </summary>
        /// <param name="container"></param>
        /// <param name="num"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        [TemplateTag]
        [XmlObjectProperty("获取热门文档列表", @"
        	<b>参数：</b><br />
        	==========================<br />
        	1：栏目Tag<br />
        	2：显示数量<br />
        	3：HTML格式
		")]
        public string HotArchives(string param, string num, string format)
        {
            //如果传入为模块编号
            if (Regex.IsMatch(param, "^\\d+$"))
            {
                int _num = 0;
                IEnumerable<ArchiveDto> dt = null;
                model.Module module = null;
                int.TryParse(num, out _num);

                module = CmsLogic.Module.GetModule(int.Parse(param));
                if (module != null)
                {
                    dt = ServiceCall.Instance.ArchiveService.GetArchivesByViewCountByModuleId(this.siteId, module.ID, _num);
                    return this.ArchiveList(dt, format);
                }
            }

            return HotArchives(param, num, format, true);
        }

        /// <summary>
        /// 按点击排行文档列表
        /// </summary>
        /// <param name="container"></param>
        /// <param name="num"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        [TemplateTag]
        [XmlObjectProperty("获取热门文档列表", @"
        	<b>参数：</b><br />
        	==========================<br />
        	1：显示数量<br />
        	2：HTML格式
		")]
        public string HotArchives(string num, string format)
        {
            string tag = HttpContext.Current.Items["category.tag"] as string;
            if (String.IsNullOrEmpty(tag))
            {
                return this.TplMessage("Error: 此标签不允许在当前页面中调用!");
            }
            return HotArchives(tag, num, format, true);
        }

        /// <summary>
        /// 按点击排行文档列表
        /// </summary>
        /// <param name="num"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        [TemplateTag]
        [XmlObjectProperty("获取热门文档列表", @"
        	<b>参数：</b><br />
        	==========================<br />
        	1：显示数量
		")]
        public string HotArchives(string num)
        {
            return HotArchives(num, base.TplSetting.CFG_ArchiveLinkFormat);
        }


        /// <summary>
        /// 按点击排行文档列表
        /// </summary>
        /// <param name="container"></param>
        /// <param name="num"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        [TemplateTag]
        [XmlObjectProperty("获取热门文档列表(不含子栏目)", @"
        	<b>参数：</b><br />
        	==========================<br />
        	1：栏目Tag<br />
        	2：显示数量<br />
        	3：HTML格式
		")]
        public string HotArchives2(string param, string num, string format)
        {
            return HotArchives(param, num, format, false);
        }

        /// <summary>
        /// 按点击排行文档列表
        /// </summary>
        /// <param name="container"></param>
        /// <param name="num"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        [TemplateTag]
        [XmlObjectProperty("获取热门文档列表(不含子栏目)", @"
        	<b>参数：</b><br />
        	==========================<br />
        	1：显示数量<br />
        	2：HTML格式
		")]
        public string HotArchives2(string num, string format)
        {
            string tag = HttpContext.Current.Items["category.tag"] as string;
            if (String.IsNullOrEmpty(tag))
            {
                return this.TplMessage("Error: 此标签不允许在当前页面中调用!");
            }
            return HotArchives(tag, num, format, false);
        }

        /// <summary>
        /// 按点击排行文档列表
        /// </summary>
        /// <param name="num"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        [TemplateTag]
        [XmlObjectProperty("获取热门文档列表(不含子栏目)", @"
        	<b>参数：</b><br />
        	==========================<br />
        	2：显示数量
		")]
        public string HotArchives2(string num)
        {
            return HotArchives2(num, base.TplSetting.CFG_ArchiveLinkFormat);
        }

        /// <summary>
        /// 根据模快获取文档
        /// </summary>
        /// <param name="num"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        [TemplateTag]
        [Obsolete]
        protected string ArchivesByCount2(string num, string format)
        {
            return "";
            object moduleID = Cms.Context.Items["module.id"];
            if (moduleID == null)
            {
                return this.TplMessage("此标签不允许在当前页面中调用!");
            }
            // return HotArchives(moduleID.ToString(),"true",num, format);
        }


        //
        //TODO:特殊文档按点击数
        //

        #endregion

        #region 栏目

        /// <summary>
        /// 栏目链接列表
        /// </summary>
        /// <param name="param"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        //[TemplateTag]
        protected string CategoryList_Old(string param, string format)
        {
            /*
            //
            // @param : 如果为int,则返回模块下的栏目，
            //                 如果为字符串tag，则返回该子类下的栏目
            //

            #region 取得栏目
            IEnumerable<Category> categories1;
            if (param == String.Empty)
            {
                categories1 = CmsLogic.Category.GetCategories();
            }
            else
            {
                if (Regex.IsMatch(param, "^\\d+$"))
                {
                    int moduleID = int.Parse(param);
                    categories1 = CmsLogic.Category.GetCategories(a => a.ModuleID == moduleID);
                }
                else
                {
                    Category c = CmsLogic.Category.Get(a =>a.SiteId==this.site.SiteId && String.Compare(a.Tag, param, true) == 0);
                    if (c != null)
                    {
                        throw new NotImplementedException();
                        //categories1 = CmsLogic.Category.getc(c.Lft, c.Rgt);
                    }
                    else
                    {
                        categories1 = null;
                    }
                }
            }
            #endregion

            if (categories1 == null) return String.Empty;
            else
            {
                IList<Category> categories = new List<Category>(categories1);
                StringBuilder sb = new StringBuilder(400);
                int i = 0;

                foreach (Category c in categories)
                {
                    sb.Append(tplengine.FieldTemplate(format, field =>
                    {
                        switch (field)
                        {
                            default: return String.Empty;

                            case "domain": return Settings.SYS_DOMAIN;

                            case "name": return c.Name;
                            case "url": return this.GetCategoryUrl(c, 1);
                            case "tag": return c.Tag;
                            case "id": return c.ID.ToString();

                            //case "pid":  return c.PID.ToString();

                            case "description": return c.Description;
                            case "keywords": return c.Keywords;
                            case "class":
                                if (i == categories.Count - 1) return " class=\"last\"";
                                else if (i == 0) return " class=\"first\"";
                                return string.Empty;
                        }
                    }));
                    ++i;
                }
                return sb.ToString();
            }
             * */
            return "";
        }

        [TemplateTag]
        [XmlObjectProperty("显示栏目列表", @"
        	参数1：栏目Tag<br />
        	参数2：显示格式
		")]
        public string Categories(string categoryTag, string format)
        {
            return base.CategoriesList(categoryTag, format);
        }

        [TemplateTag]
        [XmlObjectProperty("显示栏目列表（栏目页或文档页中）", @"
        	<b>参数：</b><br />
        	==========================<br />
        	1：显示格式
		")]
        public string Categories(string format)
        {
            string id = HttpContext.Current.Items["category.tag"] as string;
            if (String.IsNullOrEmpty(id))
            {
                return this.TplMessage("Error: 此标签不允许在当前页面中调用!");
            }
            return Categories(id, format);
        }

        [TemplateTag]
        [XmlObjectProperty("显示栏目列表（栏目页或文档页中）", @"")]
        public string Categories()
        {
            return Categories(base.TplSetting.CFG_CategoryLinkFormat);
        }

        //        [TemplateTag]
        //        [XmlObjectProperty("显示栏目(不含子栏目)列表", @"
        //        	<b>参数：</b><br />
        //        	==========================<br />
        //        	1：栏目Tag<br />
        //        	2：显示格式
        //		")]
        //        public string Categories2(string categoryTag, string format)
        //        {
        //            return Categories(categoryTag, format, !true);
        //        }

        //        [TemplateTag]
        //        [XmlObjectProperty("显示栏目(不含子栏目)列表（栏目页或文档页中）", @"
        //        	<b>参数：</b><br />
        //        	==========================<br />
        //        	1：显示格式
        //		")]
        //        public string Categories2(string format)
        //        {
        //            string id = HttpContext.Current.Items["category.tag"] as string;
        //            if (String.IsNullOrEmpty(id))
        //            {
        //                return this.TplMessage("Error: 此标签不允许在当前页面中调用!");
        //            }
        //            return Categories(id, format, !true);
        //        }

        //[TemplateTag]
        //[XmlObjectProperty("显示栏目(不含子栏目)列表（栏目页或文档页中）",@"")]
        //public string Categories2()
        //{
        //    return Categories2(base.TplSetting.CFG_CategoryLinkFormat);
        //}

        #endregion


        [TemplateTag]
        [XmlObjectProperty("显示栏目分页文档结果", @"
        	<p class=""red"">只能在栏目页或文档页中使用！</p>
        	<b>参数：</b><br />
        	==========================<br />
        	1：显示数量<br />
        	2：显示格式
		")]
        public string PagerArchives(string pageSize, string format)
        {
            string tag = HttpContext.Current.Items["category.tag"] as string;
            object pageindex = HttpContext.Current.Items["page.index"];


            if (String.IsNullOrEmpty(tag) || pageindex == null)
            {
                return this.TplMessage("Error: 此标签不允许在当前页面中调用!");
            }
            return PagerArchives(tag, pageindex.ToString(), pageSize, format);
        }




        [TemplateTag]
        [XmlObjectProperty("显示文档搜索结果", @"
        	<p class=""red"">只能在搜索页中使用！</p>
        	<b>参数：</b><br />
        	==========================<br />
        	1：显示数量<br />
        	2：显示格式
		")]
        public string SearchArchives(string pageSize, string format)
        {
            string key = HttpContext.Current.Items["search.key"] as string;
            string param = HttpContext.Current.Items["search.param"] as string;
            object pageindex = HttpContext.Current.Items["page.index"];


            if (String.IsNullOrEmpty(key) || pageindex == null)
            {
                return this.TplMessage("Error: 此标签不允许在当前页面中调用!");
            }
            return this.SearchArchives(param, key, pageindex.ToString(), pageSize, format);
        }

        [TemplateTag]
        [XmlObjectProperty("显示文档搜索结果", @"
        	<b>参数：</b><br />
        	==========================<br />
        	1：关键词<br />
        	2：显示数量<br />
        	3：显示格式
		")]
        public string SearchArchives(string keyword, string pageSize, string format)
        {
            string pageindex = HttpContext.Current.Items["page.index"] as string;
            return this.SearchArchives(null, keyword, pageindex, pageSize, format);
        }

        /// <summary>
        /// 自定义分页搜索
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="pageSize"></param>
        /// <param name="format"></param>
        /// <param name="pagerLinkPath">分页地址路径</param>
        /// <returns></returns>
        [TemplateTag]
        [XmlObjectProperty("显示文档搜索结果", @"
        	<b>参数：</b><br />
        	==========================<br />
        	1：关键词<br />
        	2：显示数量<br />
        	3：显示格式<br />
        	4：页面路径(可用于自定义搜索页的URL)
		")]
        public string SearchArchives(string keyword, string pageSize, string format, string pagerLinkPath)
        {
            int pageIndex,
                recordCount,
                pageCount;
            string c = Request("c");
            int.TryParse(Request("p"), out pageIndex);
            if (pageIndex < 1) pageIndex = 1;

            string html = this.SearchArchives(
                this.site.SiteId,
                c,
                keyword,
                pageIndex.ToString(),
                pageSize,
                format,
                out pageCount,
                out recordCount);

            //添加查询符串
            pagerLinkPath += pagerLinkPath.IndexOf("?") == -1 ? "?" : "&";

            //替换链接
            Regex reg = new Regex("([^\\?]+\\?*)(.+)", RegexOptions.IgnoreCase);

            string link1 = String.Format(TemplateUrlRule.Urls[TemplateUrlRule.RuleIndex, (int)UrlRulePageKeys.Search], HttpUtility.UrlEncode(keyword), c ?? ""),
                   link2 = String.Format(TemplateUrlRule.Urls[TemplateUrlRule.RuleIndex, (int)UrlRulePageKeys.SearchPager], HttpUtility.UrlEncode(keyword), c ?? "", "{0}");

            this.SetPager(
                        pageIndex,
                        pageCount,
                        recordCount,
                        reg.Replace(link1, String.Format("{0}$2", pagerLinkPath)),
                        reg.Replace(link2, String.Format("{0}$2", pagerLinkPath))
                    );

            return html;
        }

        /// <summary>
        /// 搜索文档列表
        /// </summary>
        /// <param name="categoryTag"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="flags">t,o,p</param>
        /// <param name="url">是否webform</param>
        /// <returns></returns>
        [TemplateTag]
        [XmlObjectProperty("显示指定栏目下的文档搜索结果", @"
        	<b>参数：</b><br />
        	==========================<br />
        	1：栏目Tag
        	2：关键词<br />
        	3：当前页码<br />
        	4：显示数量<br />
        	5：显示格式
		")]
        public string SearchArchives(string categoryTagOrModuleID, string keyword, string pageIndex, string pageSize, string format)
        {
            int pageCount, recordCount;
            return this.SearchArchives(this.site.SiteId, categoryTagOrModuleID, keyword, pageIndex, pageSize, format, out pageCount, out recordCount);
        }



        /// <summary>
        /// 链接
        /// </summary>
        /// <param name="number"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        [TemplateTag]
        [XmlObjectProperty("自定义显示链接", @"
        	<b>参数：</b><br />
        	==========================<br />
        	1：链接类型，可选[1|2|3],1:导航,2:友情链接,3:自定义链接]<br />
        	2：显示数量<br />
        	3：显示格式
		")]
        public string Links(string type, string number,string format)
        {
            return base.Link(type, format, int.Parse(number), "-1");
        }

        /// <summary>
        /// 链接
        /// </summary>
        /// <param name="number"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        [TemplateTag]
        [XmlObjectProperty("使用自定义格式显示<b>全部</b>链接", @"
        	<b>参数：</b><br />
        	==========================<br />
        	1：链接类型，可选[1|2|3],1:导航,2:友情链接,3:自定义链接]<br />
        	2：显示格式
		")]
        public string Link(string type,string format)
        {
            return base.Link(type, format, 1000, "-1");
        }

        [TemplateTag]
        [ContainSetting]
        [XmlObjectProperty("显示Tags", @"
        	<p class=""red"">仅能在文档页中使用此标签</p>
        	<b>参数：</b><br />
        	==========================<br />
        	1：标签数据，多个标签用"",""隔开。如：电脑,手机,相机
		")]
        public string Tags(string tags)
        {
            return base.Tags(tags, base.TplSetting.CFG_ArchiveTagsFormat);
        }

        [TemplateTag]
        [ContainSetting]
        [XmlObjectProperty("显示Tags", @"
        	<p class=""red"">只能在文档页中使用,格式在【模板设置】中进行设置</p>
		")]
        public string Tags()
        {
            if (!(archive.Id > 0)) return this.TplMessage("请先使用标签$require('id')获取文档后再调用属性");
            return Tags(archive.Tags, base.TplSetting.CFG_ArchiveTagsFormat);
        }

        [TemplateTag]
        [XmlObjectProperty("显示标签文档结果", @"
        	<p class=""red"">只能在标签搜索页(tags.html)中使用！</p>
        	<b>参数：</b><br />
        	==========================<br />
        	1：显示数量<br />
        	2：显示格式
		")]
        public string TagArchives(string pageSize, string format)
        {
            string key = HttpContext.Current.Items["tag.key"] as string;
            object pageindex = HttpContext.Current.Items["page.index"];

            if (String.IsNullOrEmpty(key) || pageindex == null)
            {
                return this.TplMessage("Error: 此标签不允许在当前页面中调用!");
            }
            return base.TagArchives(key, pageindex.ToString(), pageSize, format);
        }


        [TemplateTag]
        [ContainSetting]
        [XmlObjectProperty("显示评论框", @"
        	<p class=""red"">只能在文档页中使用！</p>
		")]
        public string CommentEditor()
        {
            return this.CommentEditor(base.TplSetting.CFG_AllowAmousComment ? "true" : "false",
                base.TplSetting.CFG_CommentEditorHtml);
        }



        [TemplateTag]
        [XmlObjectProperty("显示站点地图", @"
        	<p class=""red"">只能在栏目页或文档页中使用！</p>
		")]
        public string Sitemap()
        {
            string tag = Cms.Context.Items["category.tag"] as string;
            if (string.IsNullOrEmpty(tag))
            {
                return this.TplMessage("无法在当前页面调用此标签!\r\n解决方法:使用标签$sitemap('栏目标签')或设置Cms.Context.Items[\"category.tag\"]");
            }
            return Sitemap(tag);
        }


        /// <summary>
        /// 带缓存的导航
        /// </summary>
        /// <returns></returns>
        [TemplateTag]
        [ContainSetting]
        [XmlObjectProperty("显示网站导航", @"
        	<p class=""red"">显示格式在【模板设置】中修改.</p>
		")]
        public string Navigator()
        {
           string cache =  SiteLinkCache.GetNavigatorBySiteId(siteId);
           if (cache == null)
           {
               cache = base.Navigator(base.TplSetting.CFG_NavigatorLinkFormat, base.TplSetting.CFG_NavigatorChildFormat, "-1");
               SiteLinkCache.SetNavigatorForSite(siteId,cache);
           }
           return cache;
        }

        [TemplateTag]
        [ContainSetting]
        [XmlObjectProperty("显示友情链接", @"
        	<p class=""red"">显示条数，以及格式均在【模板设置】中修改.</p>
		")]
        public string FriendLink()
        {
            return this.FriendLink(base.TplSetting.CFG_FriendShowNum.ToString(), base.TplSetting.CFG_FriendLinkFormat);
        }

        [TemplateTag]
        [XmlObjectProperty("显示友情链接", @"
        	<p class=""red"">仅能在文档页中使用此标签</p>
        	<b>参数：</b><br />
        	==========================<br />
        	1：显示数目<br />
			2：HTML格式,如:<a href=""{url}"" {target}>{text}</a>
		")]
        public string FriendLink(string num, string format)
        {
            string cache = SiteLinkCache.GetFLinkBySiteId(siteId);
            if (cache == null)
            {
                cache = this.Link("2", format, int.Parse(num), "-1");
                SiteLinkCache.SetFLinkForSite(siteId, cache);
            }
            return cache;
        }



        #region 地区Region标签

        [TemplateTag]
        protected string Province(string path)
        {
            bool hasQuery = path.IndexOf('?') != -1;
            StringBuilder sb = new StringBuilder();
            sb.Append("<ul class=\"provinces\">");
            foreach (Province p in Region.Provinces)
            {
                sb.Append("<li><a href=\"").Append(path)
                    .Append(hasQuery ? "&" : "?").Append("prv=").Append(p.ID.ToString()).Append("\">")
                    .Append(p.Text).Append("</a></li>");
            }
            sb.Append("</ul>");

            return sb.ToString();
        }


        [TemplateTag]
        protected string City(string path)
        {
            bool hasQuery = path.IndexOf('?') != -1;
            int provinceID = int.Parse(this.Request("prv") ?? "1");
            StringBuilder sb = new StringBuilder();
            sb.Append("<ul class=\"cities\">");
            foreach (City p in Region.GetCities(provinceID))
            {
                sb.Append("<li><a href=\"").Append(path)
                    .Append(hasQuery ? "&" : "?").Append("prv=").Append(p.Pid.ToString()).Append("&cty=").Append(p.ID.ToString()).Append("\">")
                    .Append(p.Text).Append("</a></li>");
            }
            sb.Append("</ul>");

            return sb.ToString();
        }

        #endregion

        #region 兼容标签
        protected string PagerArchiveList(string categoryTag, string pageIndex, string pageSize, string format)
        {
            return base.PagerArchives(categoryTag, pageIndex, pageSize, format);
        }

        protected string PagerArchiveList(string pageSize, string format)
        {
            return this.PagerArchives(pageSize, format);
        }


        [TemplateTag]
        protected string SearchList(string categoryTagOrModuleID, string keyword, string pageIndex, string pageSize, string format)
        {
            int pageCount, recordCount;
            return this.SearchArchives(this.site.SiteId, categoryTagOrModuleID, keyword, pageIndex, pageSize, format, out pageCount, out recordCount);
        }

        [TemplateTag]
        protected string SearchList(string keyword, string pageSize, string format)
        {
            string pageindex = HttpContext.Current.Items["page.index"] as string;
            return SearchArchives(null, keyword, pageindex, pageSize, format);
        }

        [TemplateTag]
        protected string SearchList(string pageSize, string format)
        {
            return SearchArchives(pageSize, format);
        }

        /// <summary>
        /// 自定义分页搜索
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="pageSize"></param>
        /// <param name="format"></param>
        /// <param name="pagerLinkPath">分页地址路径</param>
        /// <returns></returns>
        [TemplateTag]
        protected string SearchList(string keyword, string pageSize, string format, string pagerLinkPath)
        {
            return this.SearchArchives(keyword, pageSize, format, pagerLinkPath);
        }



        /// <summary>
        /// 标签文档列表
        /// </summary>
        /// <param name="categoryTag"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="flags">t,o,p</param>
        /// <param name="url">是否webform</param>
        /// <returns></returns>
        [TemplateTag]
        protected string TagPagerArchiveList(string tag, string pageIndex, string pageSize, string format)
        {
            return base.TagArchives(tag, pageIndex, pageSize, format);
        }

        [TemplateTag]
        protected string TagPagerArchiveList(string pageSize, string format)
        {
            return this.TagArchives(pageSize, format);
        }


        /// <summary>
        /// 文档列表
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="container"></param>
        /// <param name="num"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        [TemplateTag]
        protected string Archives(string param, string container, string num, string format)
        {
            return this.IsTrue(container) ? this.Archives(param, num, format) : this.Archives2(param, num, format);
        }

        /// <summary>
        /// 特殊文档
        /// </summary>
        /// <param name="tag">栏目标签</param>
        /// <param name="container">是否包括子类</param>
        /// <param name="num">数量</param>
        /// <param name="format">格式
        /// </param>
        /// <returns></returns>
        [TemplateTag]
        protected string SpecialArchives(string param, string container, string num, string format)
        {
            return this.IsTrue(container) ? this.SpecialArchives(param, num, format) : this.SpecialArchives2(param, num, format);
        }

        #endregion

        #region 缩写

        /********************************  缩写  **************************/
        [TemplateTag]
        protected string SAList(string categoryTagOrModuleID, string num, string format)
        {
            return SpecialArchives(categoryTagOrModuleID, num, format);
        }

        [TemplateTag]
        protected string SList(string categoryTagOrModuleId, string keyword, string pageIndex, string pageSize, string format)
        {
            return SearchList(categoryTagOrModuleId, keyword, pageIndex, pageSize, format);
        }

        [TemplateTag]
        protected string TAList(string keyword, string pageIndex, string pageSize, string format)
        {
            return TagPagerArchiveList(keyword, pageIndex, pageSize, format);
        }

        [TemplateTag]
        protected string PAList(string categoryTag, string pageIndex, string pageSize, string format)
        {
            return PagerArchives(categoryTag, pageIndex, pageSize, format);
        }

        [TemplateTag]
        protected string CList(string param, string format)
        {
            return Categories(param, format);
        }

        [TemplateTag]
        protected string AList(string categoryTagOrModuleID, string num, string format)
        {
            return Archives(categoryTagOrModuleID, num, format, false);
        }



        [TemplateTag]
        protected string NA(string id, string format)
        {
            return NextArchive(id, format);
        }

        [TemplateTag]
        protected string PA(string id, string format)
        {
            return PrevArchive(id, format);
        }



        /******************************* 缩写结束 **************************/

        #endregion

        #region 过期

        [Obsolete]
        [TemplateTag]
        protected string ArchiveList(string param, string num, string format)
        {
            return this.Archives(param, num, format, false);
        }

        [Obsolete]
        [TemplateTag]
        protected string ArchiveList(string num, string format)
        {
            return this.Archives(num, format);
        }

        /// <summary>
        /// 根据模块ID返回
        /// </summary>
        /// <param name="num"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        [TemplateTag]
        [Obsolete]
        protected string ArchiveList2(string num, string format)
        {
            return Archives(num, format);
        }

        /// <summary>
        /// 特殊文档内容
        /// </summary>
        /// <param name="categoryTagOrModuleId"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        [TemplateTag]
        [Obsolete]
        protected string SpecialArchiveList(string categoryTagOrModuleId, string num, string format)
        {
            return SpecialArchives(categoryTagOrModuleId, num, format, false);
        }

        /// <summary>
        /// 特殊文档内容
        /// </summary>
        /// <param name="num"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        [TemplateTag]
        [Obsolete]
        protected string SpecialArchiveList(string num, string format)
        {
            return SpecialArchives(num, format);
        }

        /// <summary>
        /// 特殊文档内容
        /// </summary>
        /// <param name="num"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        [TemplateTag]
        [Obsolete]
        protected string SpecialArchiveList2(string num, string format)
        {
            return SpecialArchives2(num, format);
        }


        [Obsolete]
        [TemplateTag]
        protected string A(string idOrAlias, string format)
        {
            return Archive(idOrAlias, format);
        }




        /// <summary>
        /// 模块栏目标签
        /// </summary>
        /// <param name="id"></param>
        /// <param name="root"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        [TemplateTag]
        [Obsolete]
        protected string MCategoryList(string id, String root, string format)
        {
            IList<ICategory> categories = new List<ICategory>();
            bool onlyRoot = this.IsTrue(root);

            if (String.IsNullOrEmpty(id))
            {
                return TplMessage("请指定参数:id的值");
            }

            if (Regex.IsMatch(id, "^\\d+$"))
            {
                //从模块加载
                int moduleID = int.Parse(id);
                if (CmsLogic.Module.GetModule(moduleID) != null)
                {
                    ServiceCall.Instance.SiteService.HandleCategoryTree(this.siteId, 1, (c, level) =>
                    {
                        if (!onlyRoot || (onlyRoot && level == 0))
                        {
                            if (c.ModuleId == moduleID)
                            {
                                categories.Add(c);
                            }
                        }
                    });
                }
            }

            if (categories.Count == 0) return String.Empty;

            StringBuilder sb = new StringBuilder(400);
            int i = 0;

            foreach (ICategory c in categories.OrderBy(a => a.OrderIndex | a.Lft))
            {
                sb.Append(tplengine.FieldTemplate(format, field =>
                {
                    switch (field)
                    {
                        default: return String.Empty;

                        case "name": return c.Name;

                        //
                        //TODO:
                        //
                        //case "url": return this.GetCategoryUrl(c, 1);
                        case "tag": return c.Tag;
                        case "id": return c.Id.ToString();

                        //case "pid":  return c.PID.ToString();

                        case "description": return c.Description;
                        case "keywords": return c.Keywords;
                        case "class":
                            if (i == categories.Count - 1) return " class=\"last\"";
                            else if (i == 0) return " class=\"first\"";
                            return string.Empty;
                    }
                }));
                ++i;
            }
            return sb.ToString();

        }

        [TemplateTag]
        [Obsolete]
        protected string MCategoryList(string id, string format)
        {
            return this.MCategoryList(id, "true", format);
        }

        [TemplateTag]
        [Obsolete]
        protected string MCategoryList(string format)
        {
            object id = Cms.Context.Items["module.id"];
            if (id == null)
            {
                return this.TplMessage("此标签不允许在当前页面中调用!请使用$MCategoryList(moduleid,isRoot,format)标签代替");
            }

            return this.MCategoryList(id.ToString(), "true", format);
        }


        [Obsolete]
        protected string _MCategoryTree(string moduleID)
        {
            //读取缓存
            string cacheKey = String.Format("{0}_site{1}_mtree_{2}", CacheSign.Category.ToString(), this.siteId.ToString(), moduleID);

            BuiltCacheResultHandler<String> bh = () =>
            {
                //无缓存,则继续执行
                StringBuilder sb = new StringBuilder(400);

                int _moduleID = int.Parse(moduleID);

                //从模块加载
                if (CmsLogic.Module.GetModule(_moduleID) == null)
                {
                    return TplMessage("不存在模块!ID:" + moduleID);
                }
                sb.Append("<div class=\"category_tree mtree\">");

                CategoryDto dto = ServiceCall.Instance.SiteService.GetCategoryByLft(this.siteId, 1);

                this.CategoryTree_Iterator(dto, sb, a => { return a.ModuleId == _moduleID; }, true);

                sb.Append("</div>");

                return sb.ToString();
            };


            return Cms.Cache.GetCachedResult(cacheKey, bh);
        }


        /// <summary>
        /// 栏目链接列表
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        [TemplateTag]
        [Obsolete]
        protected string CategoryList2(string format)
        {
            return "tag install";

            object id = Cms.Context.Items["module.id"];
            if (id == null)
            {
                return this.TplMessage("此标签不允许在当前页面中调用!请使用$categorylist('categorytag','format')标签代替");
            }
            return Categories(id.ToString(), format);
        }



        #endregion



    }
}

