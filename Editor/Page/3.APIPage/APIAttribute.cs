using System;

[APIInfo("NPackage", "APIPage.APIInfoAttribute", "使用此特性修饰的类或接口，可以在API接口界面中显示自定义的描述信息。")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class APIInfoAttribute : Attribute
{
    public string GroupName { get; private set; }
    public string ClassName { get; private set; }
    public string Description { get; private set; }
    public int Order { get; private set; }


    public APIInfoAttribute(string groupName,string className,string description)
    {
        GroupName = groupName;
        ClassName = className;
        Description = description;
    }
}
