using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

[APIInfo("NPackage", "ReflectionUtil", @"
## Assembly
```csharp
public static Assembly[] GetAssemblies();
```

## Class
```csharp
// 反射获取所有继承某个接口的类
public static List<Type> GetImplementsInterfaceClass<T>();
```

## Property
```csharp
// 修改 Property 的值
public static void SetProperty<T>(this T ins, string propertyName, object propertyValue);

// 获取 Property 的值
public static T GetProperty<T>(this object ins, string propertyName);
public static object GetProperty(this object ins, string propertyName);

// 获取所有 PropertyInfo
public static PropertyInfo[] GetAllProperties(this object obj);
```

## Field
```csharp
// 获取 Field 的值
public static T GetField<T>(this object obj, string fieldName);
public static object GetField(this object obj, string fieldName);
// 修改 Field 的值
public static void SetField<T>(this object obj, string fieldName, object fieldValue);

// 获取所有 FieldInfo
public static FieldInfo[] GetAllFields(this object obj);
```

## Method
```csharp
public static void InvokeMethod(this object ins, string methodName, params object[] args);
public static T InvokeMethod<T>(this object ins, string methodName, params object[] args)
public static MethodInfo GetMethodInfo(Type ins, string methodName, params Type[] argTypes);
```

## Attribute
```csharp
public static List<T> GetAllAttribute<T>(bool inherit);
```
")]
public static class ReflectionUtil
{
	private const BindingFlags bindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
	private static readonly object[] EMPTY_PARAMS = new object[] { };

	#region Assembly
	public static Assembly[] GetAssemblies()
	{
		return AppDomain.CurrentDomain.GetAssemblies();
	}
	#endregion


	#region Class
	/// <summary>
	/// 反射获取所有继承某个接口的类
	/// </summary>
	/// <typeparam name="T">T为要查找的接口</typeparam>
	/// <returns>返回一个列表</returns>
	public static List<Type> GetImplementsInterfaceClass<T>() where T : class
	{
		Type type = typeof(T);
		var assemblies = GetAssemblies();

		var result = new List<Type>();
		for (int i = 0; i < assemblies.Length; i++)
		{
			var query = from _type in assemblies[i].GetTypes()
						where !_type.IsInterface && !_type.IsAbstract && _type.GetInterface(type.ToString()) != null
						select _type;
			foreach (Type item in query)
			{
				result.Add(item);
			}
		}

		return result;
	}
	#endregion


	#region Property
	public static void SetProperty<T>(this T ins, string propertyName, object propertyValue) where T : class
	{
		var property = ins.GetType().GetProperty(propertyName);
		if (property == null) throw new Exception($"when 'SetProperty': {ins.GetType()} dont contain property: {propertyName}");

		property?.SetValue(ins, propertyValue);
	}
	public static T GetProperty<T>(this object ins, string propertyName)
	{
		return (T)GetProperty(ins, propertyName);
	}
	public static object GetProperty(this object ins, string propertyName)
	{
		var type = ins.GetType();
		var property = type.GetProperty(propertyName);
		if (property == null) throw new Exception($"when 'GetProperty': {type} dont contain property: {propertyName}");

		return property?.GetValue(ins);
	}

	public static PropertyInfo[] GetAllProperties(this object obj)
	{
		return obj.GetType().GetProperties(bindingFlags);
	}
	#endregion


	#region Field
	public static T GetField<T>(this object obj, string fieldName)
	{
		return (T)GetField(obj, fieldName);
	}
	public static object GetField(this object obj, string fieldName)
	{
		Type type = obj.GetType();
		FieldInfo fieldInfo = type.GetField(fieldName);
		if (fieldInfo == null) throw new Exception($"when 'GetField': {type} dont contain field: {fieldName}");

		return fieldInfo?.GetValue(obj);
	}
	public static void SetField<T>(this object obj, string fieldName, object fieldValue)
	{
		Type type = obj.GetType();
		FieldInfo fieldInfo = type.GetField(fieldName);
		if (fieldInfo == null) throw new Exception($"when 'SetField': {type} dont contain field: {fieldName}");

		fieldInfo?.SetValue(obj, fieldValue);
	}
	public static FieldInfo[] GetAllFields(this object obj)
	{
		return obj.GetType().GetFields(bindingFlags);
	}
	#endregion


	#region Method
	/// <summary>
	/// 反射调用无返回值方法
	/// </summary>
	/// <param name="ins">由类(class)创建的实例化对象</param>
	/// <param name="methodName">方法名</param>
	/// <param name="args">方法传递的参数</param>
	/// <exception cref="Exception">找不到方法</exception>
	public static void InvokeMethod(this object ins, string methodName, params object[] args)
	{
		Type[] argTypes = args.Select(o => o.GetType()).ToArray();
		MethodInfo methodInfo = GetMethodInfo(ins.GetType(), methodName, argTypes);
		if (methodInfo == null) throw new Exception($"when 'InvokeMethod': {ins.GetType()} dont contain method: {methodName} with {args.Length} args");

		if (args == null || args.Length == 0) args = EMPTY_PARAMS;
		methodInfo?.Invoke(ins, args);
	}

	/// <summary>
	/// 反射调用有返回值方法
	/// </summary>
	/// <typeparam name="T">返回值类型</typeparam>
	/// <param name="ins">由类(class)创建的实例化对象</param>
	/// <param name="methodName">方法名</param>
	/// <param name="args">方法传递的参数</param>
	/// <returns>返回方法执行后的结果</returns>
	/// <exception cref="Exception">找不到方法</exception>
	public static T InvokeMethod<T>(this object ins, string methodName, params object[] args)
	{
		Type[] argTypes = args.Select(o => o.GetType()).ToArray();
		MethodInfo methodInfo = GetMethodInfo(ins.GetType(), methodName, argTypes);
		if (methodInfo == null) throw new Exception($"when 'InvokeMethod': {ins.GetType()} dont contain method: {methodName} with {args.Length} args");

		if (args == null || args.Length == 0) args = EMPTY_PARAMS;
		return (T)methodInfo?.Invoke(ins, args);
	}

	/// <summary>
	/// 获取 MethodInfo
	/// </summary>
	/// <param name="ins">class 所对应的 Type</param>
	/// <param name="methodName">方法名</param>
	/// <param name="argTypes">参数的Type</param>
	/// <returns></returns>
	public static MethodInfo GetMethodInfo(Type ins, string methodName, params Type[] argTypes)
	{
		if (argTypes == null || argTypes.Length == 0) argTypes = Type.EmptyTypes;
		return ins.GetMethod(methodName, argTypes);
	}
	#endregion


	#region CustomAttribute
	/// <summary>
	/// 获取所有自定义Attribute
	/// </summary>
	/// <typeparam name="T">要获取的Attribute，必须继承自Attribute</typeparam>
	/// <param name="inherit">是否搜索子类</param>
	/// <returns>返回搜索到的所有 <T> 类型的 Attribute</returns>
	public static List<T> GetAllAttribute<T>(bool inherit) where T: Attribute
	{
		Type type = typeof(T);
		var assemblies = GetAssemblies();

		List<T> result = new();
		for (int i = 0; i < assemblies.Length; i++)
		{
			Type[] types = assemblies[i].GetTypes();
			for (int j = 0,jMax = types.Length; j < jMax; j++)
			{
				T attribute = types[j].GetCustomAttribute<T>(inherit);
				if (attribute == null) continue;

				result.Add(attribute);
			}
		}
		return result;
	}
	#endregion

	#region CreateInstance
	public static object CreateInstance(Type type, params object[] args)
	{
		return Activator.CreateInstance(type, args);
	}
	#endregion


	#region static
	public static object GetStaticProperty(Type type, string name)
	{
		return type.GetProperty(name).GetValue(null);
	}
	public static void SetStaticProperty(Type type, string propertyName, object propertyValue)
	{
		type.GetProperty(propertyName).SetValue(null, propertyValue);
	}

	public static void InvokeStaticMethod(Type type, string methodName, params object[] args)
	{
		Type[] argTypes = args.Select(o => o.GetType()).ToArray();
		MethodInfo methodInfo = GetMethodInfo(type, methodName, argTypes);
		if (methodInfo == null) throw new Exception($"when 'InvokeStaticMethod': {type} dont contain static method: {methodName} with {args.Length} args");

		if (args == null || args.Length == 0) args = EMPTY_PARAMS;
		methodInfo?.Invoke(null, args);
	}
	#endregion
}
