//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace AutoSMS.DataModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class YM_SendList
    {
        public int Send_Id { get; set; }
        public string Send_Title { get; set; }
        public string Send_Mobile { get; set; }
        public string Send_Contents { get; set; }
        public Nullable<int> Send_TextID { get; set; }
        public Nullable<System.DateTime> Send_Time { get; set; }
        public Nullable<bool> Send_Status { get; set; }
        public Nullable<int> Send_AccountId { get; set; }
        public Nullable<bool> Send_NeedReply { get; set; }
        public Nullable<int> Send_ReplyId { get; set; }
        public Nullable<bool> Send_ReplyFlag { get; set; }
        public string Send_Result { get; set; }
        public Nullable<int> Send_Flag { get; set; }
        public Nullable<long> Send_SMSID { get; set; }
        public Nullable<bool> Send_Test { get; set; }
        public Nullable<int> Send_ReplyTemplateId { get; set; }
        public Nullable<bool> Send_IsTD { get; set; }
        public Nullable<int> Send_CreateId { get; set; }
        public Nullable<System.DateTime> Send_CreateTime { get; set; }
        public Nullable<int> Send_UpdateId { get; set; }
        public Nullable<System.DateTime> Send_UpdateDate { get; set; }
        public Nullable<int> Send_DataStatus { get; set; }
        public Nullable<bool> Send_IsPersonalized { get; set; }
        public Nullable<int> Send_Batch { get; set; }
        public string Send_ApiID { get; set; }
        public Nullable<bool> Send_ReplyIsTrue { get; set; }
        public Nullable<System.DateTime> Send_ReportDate { get; set; }
        public Nullable<bool> Send_CheckStatus { get; set; }
        public string Send_CheckId { get; set; }
        public Nullable<System.DateTime> Send_CheckTime { get; set; }
        public Nullable<int> Send_QuestionId { get; set; }
        public bool Send_IsOver { get; set; }
        public int Send_StartFlag { get; set; }
        public string Send_SubmitResult { get; set; }
        public string Seeding { get; set; }
    }
}
