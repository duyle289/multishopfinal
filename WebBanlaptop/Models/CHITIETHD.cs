//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebBanlaptop.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class CHITIETHD
    {
        public int? MASP { get; set; }
        public int? MAMAU { get; set; }
        public string MAHD { get; set; }
        public Nullable<int> SOLUONG { get; set; }
        public Nullable<decimal> DONGIA { get; set; }
    
        public virtual CHITIETSP CHITIETSP { get; set; }
        public virtual HOADON HOADON { get; set; }
        public string tenSP;
        public string tenMau;
        public virtual NHANVIEN NHANVIEN { get; set; }
        
        
    }
}
