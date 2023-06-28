<fim_prefix>// Generated by the protocol buffer compiler.  DO NOT EDIT!
// source: team_coach.proto
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using scg = global::System.Collections.Generic;
namespace PlatformHotfix {

  #region Messages
  /// <summary>
  ///主教练选择请求包
  /// </summary>
  public partial class AddCoachReq : ViewModel {
    private static readonly pb::MessageParser<AddCoachReq> _parser = new pb::MessageParser<AddCoachReq>(() => (AddCoachReq)MessagePool.Instance.Fetch(typeof(AddCoachReq)));
    public static pb::MessageParser<AddCoachReq> Parser { get { return _parser; } }

    private PropertyValue<string> englishName_;
    public PropertyValue<string> EnglishName {
      get { return englishName_; }
      set {
        englishName_ = value;
      }
    }

    private PropertyValue<string> chineseName_;
    public PropertyValue<string> ChineseName {
      get { return chineseName_; }
      set {
        chineseName_ = value;
      }
    }

    private PropertyValue<int> nationality_;
    public PropertyValue<int> Nationality {
      get { return nationality_; }
      set {
        nationality_ = value;
      }
    }

    private PropertyValue<string> nationalityName_;
    public PropertyValue<string> NationalityName {
      get { return nationalityName_; }
      set {
        nationalityName_ = value;
      }
    }

    private PropertyValue<int> gender_;
    public PropertyValue<int> Gender {
      get { return gender_; }
      set {
        gender_ = value;
      }
    }

    private PropertyValue<int> model_;
    public PropertyValue<int> Model {
      get { return model_; }
      set {
        model_ = value;
      }
    }

    private PropertyValue<int> appearance_;
    public PropertyValue<int> Appearance {
      get { return appearance_; }
      set {
        appearance_ = value;
      }
    }

    private PropertyValue<string> teamId_;
    public PropertyValue<string> TeamId {
      get { return teamId_; }
      set {
        teamId_ = value;
      }
    }

    public override void WriteTo(pb::CodedOutputStream output) {
      if (!string.IsNullOrEmpty(EnglishName.Value)) {
        output.WriteRawTag(10);
        output.WriteString(EnglishName.Value);
      }
      if (!string.IsNullOrEmpty(ChineseName.Value)) {
        output.WriteRawTag(18);
        output.WriteString(ChineseName.Value);
      }
      if (Nationality.Value!= 0) {
        output.WriteRawTag(24);
        output.WriteSInt32(Nationality.Value);
      }
      if (!string.IsNullOrEmpty(NationalityName.Value)) {
        output.WriteRawTag(34);
        output.WriteString(NationalityName.Value);
      }
      if (Gender.Value!= 0) {
        output.WriteRawTag(40);
        output.WriteSInt32(Gender.Value);
      }
      if (Model.Value!= 0) {
        output.WriteRawTag(48);
        output.WriteSInt32(Model.Value);
      }
      if (Appearance.Value!= 0) {
        output.WriteRawTag(56);
        output.WriteSInt32(Appearance.Value);
      }
      if (!string.IsNullOrEmpty(TeamId.Value)) {
        output.WriteRawTag(66);
        output.WriteString(TeamId.Value);
      }
    }

    public override int CalculateSize() {
      int size = 0;
      if (!string.IsNullOrEmpty(EnglishName.Value)) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(EnglishName.Value);
      }
      if (!string.IsNullOrEmpty(ChineseName.Value)) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(ChineseName.Value);
      }
      if (Nationality.Value!= 0) {
        size += 1 + pb::CodedOutputStream.ComputeSInt32Size(Nationality.Value);
      }
      if (!string.IsNullOrEmpty(NationalityName.Value)) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(NationalityName.Value);
      }
      if (Gender.<fim_suffix>2Size(Gender.Value);
      }
      if (Model.Value!= 0) {
        size += 1 + pb::CodedOutputStream.ComputeSInt32Size(Model.Value);
      }
      if (Appearance.Value!= 0) {
        size += 1 + pb::CodedOutputStream.ComputeSInt32Size(Appearance.Value);
      }
      if (!string.IsNullOrEmpty(TeamId.Value)) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(TeamId.Value);
      }
      return size;
    }

    public override void MergeFrom(pb::CodedInputStream input) {
      ResetDefaults();
      uint tag;
      while ((tag = input.ReadTag())!= 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 10: {
            EnglishName.Value = input.ReadString();
            break;
          }
          case 18: {
            ChineseName.Value = input.ReadString();
            break;
          }
          case 24: {
            Nationality.Value = input.ReadSInt32();
            break;
          }
          case 34: {
            NationalityName.Value = input.ReadString();
            break;
          }
          case 40: {
            Gender.Value = input.ReadSInt32();
            break;
          }
          case 48: {
            Model.Value = input.ReadSInt32();
            break;
          }
          case 56: {
            Appearance.Value = input.ReadSInt32();
            break;
          }
          case 66: {
            TeamId.Value = input.ReadString();
            break;
          }
        }
      }
    }

    public override void ResetDefaults() {
      ///english_name
      englishName_.Value = "";
      ///chinese_name
      chineseName_.Value = "";
      ///nationality
      nationality_.Value = 0;
      ///nationality_name
      nationalityName_.Value = "";
      ///gender
      gender_.Value = 0;
      ///model
      model_.Value = 0;
      ///appearance
      appearance_.Value = 0;
      ///team_id
      teamId_.Value = "";
    }

    public override void Bind(){
      EnglishName = new PropertyValue<string>(this,"EnglishName");
      ChineseName = new PropertyValue<string>(this,"ChineseName");
      Nationality = new PropertyValue<int>(this,"Nationality");
      NationalityName = new PropertyValue<string>(this,"NationalityName");
      Gender = new PropertyValue<int>(this,"Gender");
      Model = new PropertyValue<int>(this,"Model");
      Appearance = new PropertyValue<int>(this,"Appearance");
      TeamId = new PropertyValue<string>(this,"TeamId");
      BindOther();
    }

    protected override void FillProperties(System.Collections.Generic.List<ViewModelPropertyInfo> list){
      list.Add(new ViewModelPropertyInfo(EnglishName)); 
      list.Add(new ViewModelPropertyInfo(ChineseName)); 
      list.Add(new ViewModelPropertyInfo(Nationality)); 
      list.Add(new ViewModelPropertyInfo(NationalityName)); 
      list.Add(new ViewModelPropertyInfo(Gender)); 
      list.Add(new ViewModelPropertyInfo(Model)); 
      list.Add(new ViewModelPropertyInfo(Appearance)); 
      list.Add(new ViewModelPropertyInfo(TeamId)); 
      FillOtherProperties(list);
    }

    protected override void FillCommands(System.Collections.Generic.List<ViewModelCommandInfo> list)
    {
      FillOtherCommands(list); 
    }
    partial void BindOther();
    partial void FillOtherProperties(System.Collections.Generic.List<ViewModelPropertyInfo> list);
    partial void FillOtherCommands(System.Collections.Generic.List<ViewModelCommandInfo> list);
  }

  /// <summary>
  ///更新主教练名称请求包
  /// </summary>
  public partial class UpdateCoachReq : ViewModel {
    private static readonly pb::MessageParser<UpdateCoachReq> _parser = new pb::MessageParser<UpdateCoachReq>(() => (UpdateCoachReq)MessagePool.Instance.Fetch(typeof(UpdateCoachReq)));
    public static pb::MessageParser<UpdateCoachReq> Parser { get { return _parser; } }

    private PropertyValue<string> teamId_;
    public PropertyValue<string> TeamId {
      get { return teamId_; }
      set {
        teamId_ = value;
      }
    }

    private PropertyValue<string> name_;
    public PropertyValue<string> Name {
      get { return name_; }
      set {
        name_ = value;
      }
    }

    public override void WriteTo(pb::CodedOutputStream output) {
      if (!string.IsNullOrEmpty(TeamId.Value)) {
        output.WriteRawTag(10);
        output.WriteString(TeamId.Value);
      }
      if (!string.IsNullOrEmpty(Name.Value)) {
        output.WriteRawTag(18);
        output.WriteString(Name.Value);
      }
    }

    public override int CalculateSize() {
      int size = 0;
      if (!string.IsNullOrEmpty(TeamId.Value)) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(TeamId.Value);
      }
      if (!string.IsNullOrEmpty(Name.Value)) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Name.Value);
      }
      return size;
    }

    public override void MergeFrom(pb::CodedInputStream input) {
      ResetDefaults();
      uint tag;
      while ((tag = input.ReadTag())!= 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 10: {
            TeamId.Value = input.ReadString();
            break;
          }
          case 18: {
            Name.Value = input.ReadString();
            break;
          }
        }
      }
    }

    public override void ResetDefaults() {
      ///team_id
      teamId_.Value = "";
      ///name
      name_.Value = "";
    }

    public override void Bind(){
      TeamId = new PropertyValue<string>(this,"TeamId");
      Name = new PropertyValue<string>(this,"Name");
      BindOther();
    }

    protected override void FillProperties(System.Collections.Generic.List<ViewModelPropertyInfo> list){
      list.Add(new ViewModelPropertyInfo(TeamId)); 
      list.Add(new ViewModelPropertyInfo(Name)); 
      FillOtherProperties(list);
    }

    protected override void FillCommands(System.Collections.Generic.List<ViewModelCommandInfo> list)
    {
      FillOtherCommands(list); 
    }
    partial void BindOther();
    partial void FillOtherProperties(System.Collections.Generic.List<ViewModelPropertyInfo> list);
    partial void FillOtherCommands(System.Collections.Generic.List<ViewModelCommandInfo> list);
  }

  /// <summary>
  ///回包
  /// </summary>
  public partial class CoachRes : ViewModel {
    private static readonly pb::MessageParser<CoachRes> _parser = new pb::MessageParser<CoachRes>(() => (CoachRes)MessagePool.Instance.Fetch(typeof(CoachRes)));
    public static pb::MessageParser<CoachRes> Parser { get { return _parser; } }

    private PropertyValue<int> ret_;
    public PropertyValue<int> Ret {
      get { return ret_; }
      set {
        ret_ = value;
      }
    }

    public override void WriteTo(pb::CodedOutputStream output) {
      if (Ret.Value!= 0) {
        output.WriteRawTag(8);
        output.WriteSInt32(Ret.Value);
      }
    }

    public override int CalculateSize() {
      int size = 0;
      if (Ret.Value!= 0) {
        size += 1 + pb::CodedOutputStream.ComputeSInt32Size(Ret.Value);
      }
      return size;
    }

    public override void MergeFrom(pb::CodedInputStream input) {
      ResetDefaults();
      uint tag;
      while ((tag = input.ReadTag())!= 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 8: {
            Ret.Value = input.ReadSInt32();
            break;
          }
        }
      }
    }

    public override void ResetDefaults() {
      ///ret
      ret_.Value = 0;
    }

    public override void Bind(){
      Ret = new PropertyValue<int>(this,"Ret");
      BindOther();
    }

    protected override void FillProperties(System.Collections.Generic.List<ViewModelPropertyInfo> list){
      list.Add(new ViewModelPropertyInfo(Ret)); 
      FillOtherProperties(list);
    }

    protected override void FillCommands(System.Collections.Generic.List<ViewModelCommandInfo> list)
    {
      FillOtherCommands(list); 
    }
    partial void BindOther();
    partial void FillOtherProperties(System.Collections.Generic.List<ViewModelPropertyInfo> list);
    partial void FillOtherCommands(System.Collections.Generic.List<ViewModelCommandInfo> list);
  }

  #endregion

}

#endregion Designer generated code
<fim_middle>Value!= 0) {
        size += 1 + pb::CodedOutputStream.ComputeSInt3<|endoftext|>