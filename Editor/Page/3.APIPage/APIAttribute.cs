using System;

[APIInfo("NPackage", "APIPage.APIInfoAttribute", "ʹ�ô��������ε����ӿڣ�������API�ӿڽ�������ʾ�Զ����������Ϣ��")]
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
