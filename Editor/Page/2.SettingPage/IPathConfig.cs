/**
 * PathConfig V：0.0.1
 * 
 * 模块中路径管理可以继承此接口
 * 用于在框架配置界面配置各种路径操作
 * public 字段可以显示在窗口中
 * privite字段不显示
 *
 */
public interface IPathConfig
{
    string GetModuleName();
}