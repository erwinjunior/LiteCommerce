using SV20T1020136.DomainModels;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;
using System;

namespace SV20T1020136.Web
{
    /// <summary>
    /// Danh sách nhóm quyền sử dụng trong ứng dụng
    /// </summary>
    public class WebUserRole
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="name">Tên/Ký hiệu nhóm/quyền</param>
        /// <param name="description">Mô tả</param>
        public WebUserRole(string name, string description)
        {
            Name = name;
            Description = description;
        }
        /// <summary>
        /// Tên/Ký hiệu quyền
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Mô tả
        /// </summary>
        public string Description { get; set; }
    }
}
