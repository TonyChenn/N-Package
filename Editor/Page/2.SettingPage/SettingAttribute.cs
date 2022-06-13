using System;


[APIInfo("NPackage", "SettingPage.SettingPropertyAttribute","123")]
[AttributeUsage(AttributeTargets.Property)]
public class SettingPropertyAttribute : Attribute
{
    private FieldType fieldType;
    private string title;
    private string text;
    private Action action;

    public SettingPropertyAttribute(FieldType fieldType, string title)
    {
        this.fieldType = fieldType;
        this.title = title;
    }
    public SettingPropertyAttribute(string title, string text, Action action)
    {
        this.fieldType = FieldType.Button;
        this.title = title;
        this.text = text;
    }

    public string Title { get { return title; } }
    public FieldType FieldType { get { return fieldType; } }
    public string Text { get { return text; } }
    public Action Callback { get { return action; } }
}


[APIInfo("NPackage", "SettingPage.SettingMethodAttribute", "123")]
[AttributeUsage(AttributeTargets.Method)]
public class SettingMethodAttribute : Attribute
{
    private string title;
    private string text;

    public SettingMethodAttribute(string title, string text)
    {
        this.title = title;
        this.text = text;
    }

    public string Title { get { return title; } }
    public string Text { get { return text; } }
}
