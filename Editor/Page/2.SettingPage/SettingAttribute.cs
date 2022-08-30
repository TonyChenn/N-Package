using System;


[APIInfo("NPackage", "SettingPage.SettingPropertyAttribute", "使用`SettingPropertyAttribute`修饰的字段,会根据传入的`FieldType`类型在对应设置界面绘制出不同的按钮，输入框，选择框等")]
[AttributeUsage(AttributeTargets.Property)]
public class SettingPropertyAttribute : Attribute
{
    private FieldType fieldType;
    private string title;
    private string text;

    public SettingPropertyAttribute(FieldType fieldType, string title)
    {
        this.fieldType = fieldType;
        this.title = title;
    }

    public string Title { get { return title; } }
    public FieldType FieldType { get { return fieldType; } }
    public string Text { get { return text; } }
}


[APIInfo("NPackage", "SettingPage.SettingMethodAttribute", "`SettingMethodAttribute`用来在对应的设置页面绘制一个按钮，点击此按钮执行该特性标记的方法")]
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
