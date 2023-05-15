---
title: cocos2dx-luaj与android交互
date: 2021-12-03 11:15:00
updatetime: 2023-02-13 13:25:00
tags:
    - cocos2dx
    - Android
    - 多语言交互
top:
password:
description:
img: https://raw.githubusercontent.com/TonyChenn/BlogPicture/master/2021/1203/icon.jpg
---
> [cocos2dx-luaoc与ios交互](https://blog.tonychenn.cn/2021/12/27/cocos2dx-luaoc与ios交互)看这里

lua和java相互通讯，有两种方式：</br>
一种方式：是c/cpp借助JNI编写调用java方法的接口，再通过tolua++导出给lua使用。这种方法操作繁琐，效率很低。本文暂不介绍.</br>
另一种方式：利用JNI中提供的查找class, method的方式去调用java方法。(按照.net中的叫法是反射)这样就不用每次都写tolua++的转接层了。

> luaj开源仓库地址：https://github.com/luaj/luaj

特点：
- 从lua调用java方法(经过一层JNI的反射)
- 可以传递lua方法到java, 并在java中调用

在cocos2d引擎中接入了`luaj`模块，从而实现lua与java的相互调用。

一些luaj中的核心方法。
```cpp
// c/cpp call java 一些关键方法
JavaVM* jvm = cocos2d::JniHelper::getJavaVM();
jint ret = jvm->GetEnv((void**)&m_env, JNI_VERSION_1_4);
jstring _jstrClassName = m_env->NewStringUTF(m_className.c_str());
m_classID = (jclass) m_env->CallObjectMethod(cocos2d::JniHelper::classloader,
                                            cocos2d::JniHelper::loadclassMethod_methodID,
                                            _jstrClassName);
m_methodID = m_env->GetStaticMethodID(m_classID, m_methodName.c_str(), m_methodSig.c_str());

// 调用无参无返回
m_env->CallStaticVoidMethod(m_classID, m_methodID);
// 有参返回bool类型
m_ret.boolValue = m_env->CallStaticBooleanMethodA(m_classID, m_methodID, args);

// 无参返回字符串
m_retjs = (jstring)m_env->CallStaticObjectMethod(m_classID, m_methodID);
std::string strValue = cocos2d::StringUtils::getStringUTFCharsJNI(m_env, m_retjs);
m_ret.stringValue = new string(strValue);
```
cpp调用lua中的引用方法
```cpp
int LuaJavaBridge::callLuaFunctionById(int functionId, const char *arg)
{
    lua_State *L = s_luaState;
    int top = lua_gettop(L);
                                                                /* L: */
    lua_pushstring(L, LUAJ_REGISTRY_FUNCTION);                  /* L: key */
    lua_rawget(L, LUA_REGISTRYINDEX);                           /* L: f_id */
    if (!lua_istable(L, -1))
    {
        lua_pop(L, 1);
        return -1;
    }

    lua_pushnil(L); 
    while (lua_next(L, -2) != 0){                                /* L: f_id f id */
        int value = lua_tonumber(L, -1);
        lua_pop(L, 1);                                          /* L: f_id f */
        if (value == functionId){
            lua_pushstring(L, arg);                             /* L: f_id f arg */
            int ok = lua_pcall(L, 1, 1, 0);                     /* L: f_id ret|err */
            int ret;
            if (ok == 0){ ret = lua_tonumber(L, -1); }
            else { ret = -ok; }
            lua_settop(L, top);
            return ret;
        }
    }                                                           /* L: f_id */

    lua_settop(L, top);
    return -1;
}
```

cpp调用lua中的全局方法
```cpp
int LuaJavaBridge::callLuaGlobalFunction(const char *functionName, const char *arg)
{
    lua_State *L = s_luaState;
    int ret = -1;
    int top = lua_gettop(L);
    lua_getglobal(L, functionName);
    if (lua_isfunction(L, -1)){
        lua_pushstring(L, arg);
        int ok = lua_pcall(L, 1, 1, 0);
        if (ok == 0) { ret = lua_tonumber(L, -1); }
        else { ret = -ok; }
    }
    lua_settop(L, top);
    return ret;
}
```

# cocos2dx中lua调用java方法
在cocos中系统提供了`cocos.cocos2d.luaj`类，并且仅包含一个方法`callStaticMethod`，看传递的参数可以知道luaj调用java方法的方式是反射。

下面是截取luaj的源码。
```lua
local luaj = {}

local callJavaStaticMethod = LuaJavaBridge.callStaticMethod

local function checkArguments(args, sig)
    if type(args) ~= "table" then args = {} end
    if sig then return args, sig end
    -- 如果没有传sig参数会自动生成一份
    sig = {"("}
    for i, v in ipairs(args) do
        local t = type(v)
        if t == "number" then
            sig[#sig + 1] = "F"
        elseif t == "boolean" then
            sig[#sig + 1] = "Z"
        elseif t == "function" then
            sig[#sig + 1] = "I"
        else
            sig[#sig + 1] = "Ljava/lang/String;"
        end
    end
    sig[#sig + 1] = ")V"

    return args, table.concat(sig)
end

-- className    类名,即（包名.类名）
-- methodName   调用的方法名
-- args         传递的参数
-- sig          签名
function luaj.callStaticMethod(className, methodName, args, sig)
    local args, sig = checkArguments(args, sig)
    return callJavaStaticMethod(className, methodName, args, sig)
end

return luaj
```

## 关于sig参数
之前一直没搞明白，看到`checkArguments`方法才明白了。sig参数分两部分：</br>
如：`()Ljava/lang/String;`</br>括号内是<kbd>参数类型</kbd>，括号外是<kbd>返回值
类型</kbd>。</br>这个例子是一个没有参数，带一个其他类型返回值的签名。


如果参数为空，返回值为空，则sig为 `()V`

## 参数类型与sig对应表
|参数类型|sig|
|---|---|
|number|F|
|boolean|Z|
|function/整数|I(H I J中的I)|
|其他|Ljava/lang/String;|

## 使用方法
如下是一个调用AndroidToast的方法：
- lua调用java方法
```lua
function MainScene:CallAndroidToast(msg)
    if device.platform == "android" then
        print("是Android工程")
        local luaj = require "cocos.cocos2d.luaj"
        luaj.callStaticMethod("org.cocos2dx.lua.AppActivity", "ShowToast", {msg}, "(Ljava/lang/String;)V")
    end
end
```
- java 方法
```java
public static void ShowToast(final String msg){
    Log.d("cocos", "到java method 了");
    mContext.runOnUiThread(new Runnable(){
        @Override
        public void run() {
            Toast.makeText(mContext, msg, Toast.LENGTH_LONG).show();
        }
    });
}
```



# java调用lua方法
同样cocos也为我们提供了`Cocos2dxLuaJavaBridge`类，包含了四个接口,每个接口的含义很清晰就不再赘述。
```java
package org.cocos2dx.lib;

public class Cocos2dxLuaJavaBridge
{
    public static native int callLuaFunctionWithString(int luaFunctionId, String value);
    public static native int callLuaGlobalFunctionWithString(String luaFunctionName, String value);
    public static native int retainLuaFunction(int luaFunctionId);
    public static native int releaseLuaFunction(int luaFunctionId);
}
```
具体实现写在了cpp层，我们看下，做了一下绑定，里面也是调用的`LuaJavaBridge`类中的方法。
```cpp
// /frameworks/cocos2d-x/cocos/scripting/lua-bindings/manual/platform/android/jni/Java_org_cocos2dx_lib_Cocos2dxLuaJavaBridge.cpp

// extern "C" 的作用是告诉编译器以C语言的形式编译这些代码
extern "C" {
// 看了下和Java中调用方法名关系，应该是导出后的方法名是这样的组成：
// Java_PackageName_ClassName_FunctionName
JNIEXPORT jint JNICALL Java_org_cocos2dx_lib_Cocos2dxLuaJavaBridge_callLuaFunctionWithString
  (JNIEnv *env, jclass cls, jint functionId, jstring value)
{
    std::string strValue = cocos2d::StringUtils::getStringUTFCharsJNI(env, value);
    int ret = LuaJavaBridge::callLuaFunctionById(functionId, strValue.c_str());
    return ret;
}
}
```

## 使用
如上面，假如lua向java侧传递了一个方法作为参数，java侧要如何进行调用呢？
```java
// 执行拍照，传递一个保存图片后的Callback方法
// luaSaveImgFunc lua 方法
public static void handle_takeCameraPic(int luaSaveImgFunc) {
    luaSaveImageCallBack = luaSaveImgFunc;
    ...
}
// 执行拍照完成后调用lua回调
public static void ExecLuaTakeImgCallBack(final String msg){
    if(luaSaveImageCallBack != 0){
        getInstance().mActivity.runOnGLThread(new Runnable() {
            @Override
            public void run() {
                // 调用lua方法
                Cocos2dxLuaJavaBridge.callLuaFunctionWithString(luaSaveImageCallBack, msg);
                // 释放方法引用
                Cocos2dxLuaJavaBridge.releaseLuaFunction(luaSaveImageCallBack);
                luaSaveImageCallBack = 0;
            }
        });
    }
}
```
</br>
</br>

# 最后附上完整的LuaJavaBridge源码
```cpp
// cocos/scripting/lua-bindings/manual/platform/android/CCLuaJavaBridge.cpp
#include "CCLuaJavaBridge.h"
#include "platform/android/jni/JniHelper.h"
#include <android/log.h>
#include "base/ccUTF8.h"

#define  LOG_TAG    "luajc"
#define  LOGD(...)  __android_log_print(ANDROID_LOG_DEBUG,LOG_TAG,__VA_ARGS__)

extern "C" {
#include "tolua_fix.h"
}

LuaJavaBridge::CallInfo::~CallInfo(void)
{
	if (m_returnType == TypeString && m_ret.stringValue)
	{
		delete m_ret.stringValue;
	}
}

bool LuaJavaBridge::CallInfo::execute(void)
{
	switch (m_returnType)
    {
        case TypeVoid:
            m_env->CallStaticVoidMethod(m_classID, m_methodID);
            break;

        case TypeInteger:
            m_ret.intValue = m_env->CallStaticIntMethod(m_classID, m_methodID);
            break;

        case TypeFloat:
            m_ret.floatValue = m_env->CallStaticFloatMethod(m_classID, m_methodID);
            break;

        case TypeBoolean:
            m_ret.boolValue = m_env->CallStaticBooleanMethod(m_classID, m_methodID);
            break;

        case TypeString:
            m_retjs = (jstring)m_env->CallStaticObjectMethod(m_classID, m_methodID);
            std::string strValue = cocos2d::StringUtils::getStringUTFCharsJNI(m_env, m_retjs);
            m_ret.stringValue = new string(strValue);
           break;
    }

	if (m_env->ExceptionCheck() == JNI_TRUE)
	{
		m_env->ExceptionDescribe();
		m_env->ExceptionClear();
		m_error = LUAJ_ERR_EXCEPTION_OCCURRED;
		return false;
	}

	return true;
}


bool LuaJavaBridge::CallInfo::executeWithArgs(jvalue *args)
{
    switch (m_returnType)
     {
         case TypeVoid:
             m_env->CallStaticVoidMethodA(m_classID, m_methodID, args);
             break;

         case TypeInteger:
             m_ret.intValue = m_env->CallStaticIntMethodA(m_classID, m_methodID, args);
             break;

         case TypeFloat:
             m_ret.floatValue = m_env->CallStaticFloatMethodA(m_classID, m_methodID, args);
             break;

         case TypeBoolean:
             m_ret.boolValue = m_env->CallStaticBooleanMethodA(m_classID, m_methodID, args);
             break;

         case TypeString:
        	 m_retjs = (jstring)m_env->CallStaticObjectMethodA(m_classID, m_methodID, args);
            std::string strValue = cocos2d::StringUtils::getStringUTFCharsJNI(m_env, m_retjs);
            m_ret.stringValue = new string(strValue);
            break;
     }

	if (m_env->ExceptionCheck() == JNI_TRUE)
	{
		m_env->ExceptionDescribe();
		m_env->ExceptionClear();
		m_error = LUAJ_ERR_EXCEPTION_OCCURRED;
		return false;
	}

	return true;
}

int LuaJavaBridge::CallInfo::pushReturnValue(lua_State *L)
{
	if (m_error != LUAJ_ERR_OK)
	{
		lua_pushinteger(L, m_error);
		return 1;
	}

	switch (m_returnType)
	{
		case TypeInteger:
			lua_pushinteger(L, m_ret.intValue);
			return 1;
		case TypeFloat:
			lua_pushnumber(L, m_ret.floatValue);
			return 1;
		case TypeBoolean:
			lua_pushboolean(L, m_ret.boolValue);
			return 1;
		case TypeString:
			lua_pushstring(L, m_ret.stringValue->c_str());
			return 1;
	}

	return 0;
}

/*
确认方法的参数,解析参数中类型
将其转换为java认可的类型存入m_argumentsType和m_returnType中
*/
bool LuaJavaBridge::CallInfo::validateMethodSig(void)
{
    //获取方法参数的长度
    //最小的方法是"()V",如果不满足则认为传参错误
    size_t len = m_methodSig.length();
    if (len < 3 || m_methodSig[0] != '(') // min sig is "()V"
    {
    	m_error = LUAJ_ERR_INVALID_SIGNATURES;
    	return false;
	}

    size_t pos = 1;//访问字符串的位置
    //下面这个循环是解析lua往java方向传入的参数类型
    while (pos < len && m_methodSig[pos] != ')')
    {
    	LuaJavaBridge::ValueType type = checkType(m_methodSig, &pos);
    	if (type == TypeInvalid) return false;

		m_argumentsCount++;
        //将兑换以后的类型存起来
		m_argumentsType.push_back(type);
        pos++;
    }

    if (pos >= len || m_methodSig[pos] != ')')
	{
    	m_error = LUAJ_ERR_INVALID_SIGNATURES;
    	return false;
	}

    pos++;
    //获取java向lua端的参数类型
    m_returnType = checkType(m_methodSig, &pos);
    return true;
}

/*
检查类型
我们在lua端往java端传值的类型如下：
I:--------------------》TypeInteger
F:--------------------》TypeFloat
Z:--------------------》TypeBoolean
V:--------------------》TypeVoid
Ljava/lang/String;----》TypeString
Ljava/util/Vector;----》TypeVector
本函数从lua函数以后会按照上面提到的类型进行兑换
*/
LuaJavaBridge::ValueType LuaJavaBridge::CallInfo::checkType(const string& sig, size_t *pos)
{
    switch (sig[*pos])
    {
        case 'I':
            return TypeInteger;//整形
        case 'F':
            return TypeFloat;//浮点型
        case 'Z':
            return TypeBoolean;//bool值
        case 'V':
        	return TypeVoid;//空值
        case 'L':
            size_t pos2 = sig.find_first_of(';', *pos + 1);
            if (pos2 == string::npos)
            {
                m_error = LUAJ_ERR_INVALID_SIGNATURES;
                return TypeInvalid;
            }

            const string t = sig.substr(*pos, pos2 - *pos + 1);
            if (t.compare("Ljava/lang/String;") == 0)
            {
            	*pos = pos2;
                return TypeString;
            }
            else if (t.compare("Ljava/util/Vector;") == 0)
            {
            	*pos = pos2;
                return TypeVector;
            }
            else
            {
            	m_error = LUAJ_ERR_TYPE_NOT_SUPPORT;
                return TypeInvalid;
            }
    }

    m_error = LUAJ_ERR_TYPE_NOT_SUPPORT;
    return TypeInvalid;
}

/*
获取方法的信息
m_classID :
m_methodID :
m_env :
*/
bool LuaJavaBridge::CallInfo::getMethodInfo(void)
{
    m_methodID = 0;
    m_env = 0;

    JavaVM* jvm = cocos2d::JniHelper::getJavaVM();
    jint ret = jvm->GetEnv((void**)&m_env, JNI_VERSION_1_4);
    switch (ret) {
        case JNI_OK:
            break;

        case JNI_EDETACHED :
            if (jvm->AttachCurrentThread(&m_env, NULL) < 0)
            {
                LOGD("%s", "Failed to get the environment using AttachCurrentThread()");
                m_error = LUAJ_ERR_VM_THREAD_DETACHED;
                return false;
            }
            break;

        case JNI_EVERSION :
        default :
            LOGD("%s", "Failed to get the environment using GetEnv()");
            m_error = LUAJ_ERR_VM_FAILURE;
            return false;
    }
    jstring _jstrClassName = m_env->NewStringUTF(m_className.c_str());
    m_classID = (jclass) m_env->CallObjectMethod(cocos2d::JniHelper::classloader,
                                                   cocos2d::JniHelper::loadclassMethod_methodID,
                                                   _jstrClassName);

    if (NULL == m_classID) {
        LOGD("Classloader failed to find class of %s", m_className.c_str());
    }

    m_env->DeleteLocalRef(_jstrClassName);
    m_methodID = m_env->GetStaticMethodID(m_classID, m_methodName.c_str(), m_methodSig.c_str());
    if (!m_methodID)
    {
        m_env->ExceptionClear();
        LOGD("Failed to find method id of %s.%s %s",
                m_className.c_str(),
                m_methodName.c_str(),
                m_methodSig.c_str());
        m_error = LUAJ_ERR_METHOD_NOT_FOUND;
        return false;
    }

    return true;
}

/* ---------------------------------------- */

lua_State *LuaJavaBridge::s_luaState = NULL;
int LuaJavaBridge::s_newFunctionId = 0;

void LuaJavaBridge::luaopen_luaj(lua_State *L)
{
	s_luaState = L;
    lua_newtable(L);
    lua_pushstring(L, "callStaticMethod");
    lua_pushcfunction(L, LuaJavaBridge::callJavaStaticMethod);
    lua_rawset(L, -3);
    lua_setglobal(L, "LuaJavaBridge");
}

/*
args:
    const char *className
    const char *methodName
    LUA_TABLE   args
    const char *sig
*/
int LuaJavaBridge::callJavaStaticMethod(lua_State *L)
{
    if (!lua_isstring(L, -4) || !lua_isstring(L, -3)  || !lua_istable(L, -2) || !lua_isstring(L, -1))
    {
    	lua_pushboolean(L, 0);
    	lua_pushinteger(L, LUAJ_ERR_INVALID_SIGNATURES);
    	return 2;
    }

    LOGD("%s", "LuaJavaBridge::callJavaStaticMethod(lua_State *L)");
    /*
        虚拟机在执行下面这句话的时候，会依次向栈中push参数（函数首地址，className,javaMethodName,args,sigs）
        luaj.callStaticMethod(className,javaMethodName,args,sigs)
        此时栈中的元素分布如下
        L->top  -----> null
        L->top-1-----> methodSig
        L->top-2-----> args
        L->top-3-----> methodName
        L->top-4-----> className
        L->base -----> callJavaStaticMethod(函数的首地址)
    */

    const char *className  = lua_tostring(L, -4);
    const char *methodName = lua_tostring(L, -3);
    const char *methodSig  = lua_tostring(L, -1);
    
    //生成一个call对象
    //CallInfo这个类被包含在LuaJavaBridge中用于处理数据
    //除此之外LuaJavaBridge包含的方法全是静态方法
    CallInfo call(className, methodName, methodSig);

    // check args 弹出栈顶元素methodSig
    // 弹出以后 栈顶元素指向了args
    lua_pop(L, 1);
    

    /*
       下面这个函数其实就是取出args中的参数，并且将他们全部入栈
       最后返回table元素的个数
    */													/* L: args */
    int count = fetchArrayElements(L, -1);                      	/* L: args e1 e2 e3 e4 ... */

    jvalue *args = NULL;
    if (count > 0)
    {
	    args = new jvalue[count];
        //遍历所有args的参数
	    for (int i = 0; i < count; ++i)
	    {
            //参数是正着入栈的
            //取得时候反着来，这样和之前传参的时候一一对应
	        int index = -count + i;
            /*
               call.argumentTypeAtIndex(i)
               这个方法是去获取参数的类型
            */
	        switch (call.argumentTypeAtIndex(i))
	        {
	            case TypeInteger:
	            	if (lua_isfunction(L, index))
	            	{
	                    args[i].i = retainLuaFunction(L, index, NULL);
	            	}
	            	else
	            	{
	            		args[i].i = (int)lua_tonumber(L, index);
	            	}
	                break;

	            case TypeFloat:
	                args[i].f = lua_tonumber(L, index);
	                break;

	            case TypeBoolean:
	                args[i].z = lua_toboolean(L, index) != 0 ? JNI_TRUE : JNI_FALSE;
	                break;

	            case TypeString:
	            default:
	                args[i].l = call.getEnv()->NewStringUTF(lua_tostring(L, index));
	                break;
	        }
	    }
        //从栈中弹出所有的参数
	    lua_pop(L, count);                               			/* L: args */
    }

    bool success = args ? call.executeWithArgs(args) : call.execute();
    if (args) delete []args;

    if (!success)
    {
    	LOGD("LuaJavaBridge::callJavaStaticMethod(\"%s\", \"%s\", args, \"%s\") EXECUTE FAILURE, ERROR CODE: %d",
    			className, methodName, methodSig, call.getErrorCode());

    	lua_pushboolean(L, 0);
    	lua_pushinteger(L, call.getErrorCode());
    	return 2;
    }

	LOGD("LuaJavaBridge::callJavaStaticMethod(\"%s\", \"%s\", args, \"%s\") SUCCESS",
			className, methodName, methodSig);

	lua_pushboolean(L, 1);
	return 1 + call.pushReturnValue(L);
}

// increase lua function reference counter, return counter
int LuaJavaBridge::retainLuaFunctionById(int functionId)
{
    lua_State *L = s_luaState;

    lua_pushstring(L, LUAJ_REGISTRY_RETAIN);                    /* L: key */
    lua_rawget(L, LUA_REGISTRYINDEX);                           /* L: id_r */
    if (!lua_istable(L, -1))
    {
        lua_pop(L, 1);
        return 0;
    }

    // get counter
    lua_pushinteger(L, functionId);                             /* L: id_r id */
    lua_rawget(L, -2);                                          /* L: id_r r */
    if (lua_type(L, -1) != LUA_TNUMBER)
    {
        lua_pop(L, 2);
        return 0;
    }

    // increase counter
    int retainCount = lua_tonumber(L, -1);
    retainCount++;
    lua_pop(L, 1);                                              /* L: id_r */
    lua_pushinteger(L, functionId);                             /* L: id_r id */
    lua_pushinteger(L, retainCount);                            /* L: id_r id r */
    lua_rawset(L, -3);                            /* id_r[id] = r, L: id_r */
    lua_pop(L, 1);

    LOGD("luajretainLuaFunctionById(%d) - retain count = %d", functionId, retainCount);

    return retainCount;
}

// decrease lua function reference counter, return counter
int LuaJavaBridge::releaseLuaFunctionById(int functionId)
{
    lua_State *L = s_luaState;
                                                                /* L: */
    lua_pushstring(L, LUAJ_REGISTRY_FUNCTION);                  /* L: key */
    lua_rawget(L, LUA_REGISTRYINDEX);                           /* L: f_id */
    if (!lua_istable(L, -1))
    {
        lua_pop(L, 1);
        LOGD("%s", "luajreleaseLuaFunctionById() - LUAJ_REGISTRY_FUNCTION not exists");
        return 0;
    }

    lua_pushstring(L, LUAJ_REGISTRY_RETAIN);                    /* L: f_id key */
    lua_rawget(L, LUA_REGISTRYINDEX);                           /* L: f_id id_r */
    if (!lua_istable(L, -1))
    {
        lua_pop(L, 2);
        LOGD("%s", "luajreleaseLuaFunctionById() - LUAJ_REGISTRY_RETAIN not exists");
        return 0;
    }

    lua_pushinteger(L, functionId);                             /* L: f_id id_r id */
    lua_rawget(L, -2);                                          /* L: f_id id_r r */
    if (lua_type(L, -1) != LUA_TNUMBER)
    {
        lua_pop(L, 3);
        LOGD("luajreleaseLuaFunctionById() - function id %d not found", functionId);
        return 0;
    }

    int retainCount = lua_tonumber(L, -1);
    retainCount--;

    if (retainCount > 0)
    {
        // update counter
        lua_pop(L, 1);                                          /* L: f_id id_r */
        lua_pushinteger(L, functionId);                         /* L: f_id id_r id */
        lua_pushinteger(L, retainCount);                        /* L: f_id id_r id r */
        lua_rawset(L, -3);                        /* id_r[id] = r, L: f_id id_r */
        lua_pop(L, 2);
        LOGD("luajreleaseLuaFunctionById() - function id %d retain count = %d", functionId, retainCount);
        return retainCount;
    }

    // remove lua function reference
    lua_pop(L, 1);                                              /* L: f_id id_r */
    lua_pushinteger(L, functionId);                             /* L: f_id id_r id */
    lua_pushnil(L);                                             /* L: f_id id_r id nil */
    lua_rawset(L, -3);                          /* id_r[id] = nil, L: f_id id_r */

    lua_pop(L, 1);                                              /* L: f_id */
    lua_pushnil(L);                                             /* L: f_id nil */
    while (lua_next(L, -2) != 0)                                /* L: f_id f id */
    {
        int value = lua_tonumber(L, -1);
        lua_pop(L, 1);                                          /* L: f_id f */
        if (value == functionId)
        {
            lua_pushnil(L);                                     /* L: f_id f nil */
            lua_rawset(L, -3);                   /* f_id[f] = nil, L: f_id */
            break;
        }
    }                                                           /* L: f_id */

    lua_pop(L, 1);
    LOGD("luajreleaseLuaFunctionById() - function id %d released", functionId);
    return 0;
}

int LuaJavaBridge::callLuaFunctionById(int functionId, const char *arg)
{
    lua_State *L = s_luaState;
    int top = lua_gettop(L);
                                                                /* L: */
    lua_pushstring(L, LUAJ_REGISTRY_FUNCTION);                  /* L: key */
    lua_rawget(L, LUA_REGISTRYINDEX);                           /* L: f_id */
    if (!lua_istable(L, -1))
    {
        lua_pop(L, 1);
        return -1;
    }

    lua_pushnil(L); 
    /*
    L->top    null
    L->top-1  nil
    L->top-2  G(L)->l_registry[LUAJ_REGISTRY_FUNCTION]
    ...
    */ 
    //寻找G(L)->l_registry[LUAJ_REGISTRY_FUNCTION]它的所有(key value)
                                                 /* L: f_id nil */
    while (lua_next(L, -2) != 0)                                /* L: f_id f id */
    {
         /*
        L->top    null
        L->top-1  value
        L->top-2  下一次寻找的key
        L->top-3  G(L)->l_registry[LUAJ_REGISTRY_FUNCTION]
        ...
        */ 
        int value = lua_tonumber(L, -1);
        lua_pop(L, 1);                                          /* L: f_id f */
         /*
        L->top    null
        L->top-1  下一次寻找的key
        L->top-2  G(L)->l_registry[LUAJ_REGISTRY_FUNCTION]
        ...
        */ 
        if (value == functionId)
        {
            lua_pushstring(L, arg);                             /* L: f_id f arg */
            int ok = lua_pcall(L, 1, 1, 0);                     /* L: f_id ret|err */
            int ret;
            if (ok == 0)
            {
                ret = lua_tonumber(L, -1);
            }
            else
            {
                ret = -ok;
            }

            lua_settop(L, top);
            return ret;
        }
    }                                                           /* L: f_id */

    lua_settop(L, top);
    return -1;
}

// call lua global function
int LuaJavaBridge::callLuaGlobalFunction(const char *functionName, const char *arg)
{
    lua_State *L = s_luaState;

    int ret = -1;
    int top = lua_gettop(L);

    lua_getglobal(L, functionName);
    if (lua_isfunction(L, -1))
    {
        lua_pushstring(L, arg);
        int ok = lua_pcall(L, 1, 1, 0);
        if (ok == 0)
        {
            ret = lua_tonumber(L, -1);
        }
        else
        {
            ret = -ok;
        }
    }

    lua_settop(L, top);
    return ret;
}

// ----------------------------------------

// increase lua function reference counter, return functionId
/*
下面的函数的意思就是要把luaFunc注册到G(L)->l_registry[LUAJ_REGISTRY_FUNCTION]中
并且对于同样一个函数记录它注册的次数
G(L)->l_registry[LUAJ_REGISTRY_FUNCTION][luaFunc] = functionId
G(L)->l_registry[LUAJ_REGISTRY_RETAIN][functionId] = retainCount
*/
int LuaJavaBridge::retainLuaFunction(lua_State *L, int functionIndex, int *retainCountReturn)
{
    /*
    压如一个字符串LUAJ_REGISTRY_FUNCTION  栈的高度加1
    *1                                                            /* L: f ... */
    lua_pushstring(L, LUAJ_REGISTRY_FUNCTION);                  /* L: f ... key */
    /*
    将G(L)->l_registry[LUAJ_REGISTRY_FUNCTION]放到栈顶 栈的大小不变
    */
    lua_rawget(L, LUA_REGISTRYINDEX);                           /* L: f ... f_id */
    if (!lua_istable(L, -1))
    {
        /*
        如果栈顶不是table 则弹出栈顶元素 
        */
        lua_pop(L, 1);
        //压入一个table
        lua_newtable(L);
        //压入一个字符串
        lua_pushstring(L, LUAJ_REGISTRY_FUNCTION);
        //将table放到栈顶
        lua_pushvalue(L, -2);
        /*
        此时栈中情况如下
        L->top    null
        L->top-1  table
        L->top-2  LUAJ_REGISTRY_FUNCTION
        L->top-3  table
        ...
        */
        //下面这句话
        // G(L)->l_registry[LUAJ_REGISTRY_FUNCTION] = table
        lua_rawset(L, LUA_REGISTRYINDEX);
        /*
            此时栈中的情况是
            L->top 
            L->top-1 G(L)->l_registry[LUAJ_REGISTRY_FUNCTION]
        */
    }

    lua_pushstring(L, LUAJ_REGISTRY_RETAIN);                    /* L: f ... f_id key */
    lua_rawget(L, LUA_REGISTRYINDEX);                           /* L: f ... f_id id_r */
    if (!lua_istable(L, -1))
    {
        lua_pop(L, 1);
        lua_newtable(L);
        lua_pushstring(L, LUAJ_REGISTRY_RETAIN);
        lua_pushvalue(L, -2);
        //G(L)->l_registry[LUAJ_REGISTRY_RETAIN] = table
        lua_rawset(L, LUA_REGISTRYINDEX);
    }



    // get function id
    //经过上面两个操作以后栈的高度加了2
    /*
       L->top null
       L->top-1 G(L)->l_registry[LUAJ_REGISTRY_RETAIN]
       L->top-2 G(L)->l_registry[LUAJ_REGISTRY_FUNCTION]
       下面栈的内容就和刚进来的时候一样了
       所以此处获取函数的时候，将之前的索引-2
       将目标函数放到栈顶(L->top-1)位置，栈高度加一
    */
    lua_pushvalue(L, functionIndex - 2);                        /* L: f ... f_id id_r f */
    /*
    此时栈中的内容
    L->top null
    L->top-1 luaFunc  每一个函数的地址都是唯一的
    L->top-2 G(L)->l_registry[LUAJ_REGISTRY_RETAIN]
    L->top-3 G(L)->l_registry[LUAJ_REGISTRY_FUNCTION]
    下面栈的内容就和刚进来的时候一样了
    */
    lua_rawget(L, -3);                                          /* L: f ... f_id id_r id */
    /*
    此时栈中内容
    L->top null
    L->top-1 G(L)->l_registry[LUAJ_REGISTRY_FUNCTION][luaFunc]
    L->top-2 G(L)->l_registry[LUAJ_REGISTRY_RETAIN]
    L->top-3 G(L)->l_registry[LUAJ_REGISTRY_FUNCTION]
    下面栈的内容就和刚进来的时候一样了
    */

    int functionId;
    if (lua_type(L, -1) != LUA_TNUMBER)
    {
        // first retain, create new id
        lua_pop(L, 1);                                          /* L: f ... f_id id_r */

        s_newFunctionId++;
        functionId = s_newFunctionId;
        /*
           L->top null
           L->top-1 G(L)->l_registry[LUAJ_REGISTRY_RETAIN]
           L->top-2 G(L)->l_registry[LUAJ_REGISTRY_FUNCTION]
           下面栈的内容就和刚进来的时候一样了
           所以此处获取函数的时候，将之前的索引-2
           将目标函数放到栈顶(L->top-1)位置，栈高度加一
       */
        lua_pushvalue(L, functionIndex - 2);                    /* L: f ... f_id id_r f */
        lua_pushinteger(L, functionId);                         /* L: f ... f_id id_r f id */
        /*
           L->top null
           L->top-1 functionId
           L->top-2 luaFunc
           L->top-3 G(L)->l_registry[LUAJ_REGISTRY_RETAIN]
           L->top-4 G(L)->l_registry[LUAJ_REGISTRY_FUNCTION]
           下面栈的内容就和刚进来的时候一样了
        */
        //G(L)->l_registry[LUAJ_REGISTRY_FUNCTION][luaFunc] = functionId
        //栈的高度-2
        lua_rawset(L, -4);                        /* f_id[f] = id, L: f ... f_id id_r */
        /*
           L->top null
           L->top-1 G(L)->l_registry[LUAJ_REGISTRY_RETAIN]
           L->top-2 G(L)->l_registry[LUAJ_REGISTRY_FUNCTION]
           下面栈的内容就和刚进来的时候一样了
       */
        lua_pushinteger(L, functionId);                         /* L: f ... f_id id_r id */
        /*
           L->top null
           L->top-1 functionId
           L->top-2 G(L)->l_registry[LUAJ_REGISTRY_RETAIN]
           L->top-3 G(L)->l_registry[LUAJ_REGISTRY_FUNCTION]
           下面栈的内容就和刚进来的时候一样了
       */
    }
    else
    {
        functionId = lua_tonumber(L, -1);
        /*
           L->top null
           L->top-1 functionId
           L->top-2 G(L)->l_registry[LUAJ_REGISTRY_RETAIN]
           L->top-3 G(L)->l_registry[LUAJ_REGISTRY_FUNCTION]
           下面栈的内容就和刚进来的时候一样了
       */
    }

    // get function retain
    lua_pushvalue(L, -1);                                       /* L: f ... f_id id_r id id */
    /*
       L->top null
       L->top-1 functionId
       L->top-2 functionId
       L->top-3 G(L)->l_registry[LUAJ_REGISTRY_RETAIN]
       L->top-4 G(L)->l_registry[LUAJ_REGISTRY_FUNCTION]
       下面栈的内容就和刚进来的时候一样了
    */
    lua_rawget(L, -3);                                          /* L: f ... f_id id_r id r */
    /*
       L->top null
       L->top-1 G(L)->l_registry[LUAJ_REGISTRY_RETAIN][functionId]
       L->top-2 functionId
       L->top-3 G(L)->l_registry[LUAJ_REGISTRY_RETAIN]
       L->top-4 G(L)->l_registry[LUAJ_REGISTRY_FUNCTION]
       下面栈的内容就和刚进来的时候一样了
    */
    int retainCount = 1;
    if (lua_type(L, -1) != LUA_TNUMBER)
    {
        // first retain, set retain count = 1
        lua_pop(L, 1);
        lua_pushinteger(L, retainCount);
        /*
           L->top null
           L->top-1 retainCount
           L->top-2 functionId
           L->top-3 G(L)->l_registry[LUAJ_REGISTRY_RETAIN]
           L->top-4 G(L)->l_registry[LUAJ_REGISTRY_FUNCTION]
           下面栈的内容就和刚进来的时候一样了
       */
    }
    else
    {
        // add retain count
        retainCount = lua_tonumber(L, -1);
        retainCount++;
        lua_pop(L, 1);
        lua_pushinteger(L, retainCount);
    }
    //G(L)->l_registry[LUAJ_REGISTRY_RETAIN][functionId] = retainCount
    lua_rawset(L, -3);                            /* id_r[id] = r, L: f ... f_id id_r */
    /*
       L->top null
       L->top-1 G(L)->l_registry[LUAJ_REGISTRY_RETAIN]
       L->top-2 G(L)->l_registry[LUAJ_REGISTRY_FUNCTION]
       下面栈的内容就和刚进来的时候一样了
    */

    lua_pop(L, 2);                                              /* L: f ... */

    /*
       最终栈的内容和刚进来一样
       L->top null
       下面栈的内容就和刚进来的时候一样了
    */


    if (retainCountReturn) *retainCountReturn = retainCount;
    return functionId;
}

//取出数组元素
//取出栈中指定索引index的table,并且把这个table的所有value值全部入栈
int LuaJavaBridge::fetchArrayElements(lua_State *L, int index)
{
    int count = 0;
    do
    {
        /*
          取出栈中指定位置的table,并且把它的table指定位置的value入栈
          所以栈的大小会不断加大，要想获取最原始的table,需要调整索引(index - count)
          而table的值则是默认自增取（1,2,3...n）
        */
        lua_rawgeti(L, index - count, count + 1);
        if (lua_isnil(L, -1))
        {
            //如果判断栈顶元素为空，说明取完了啊，弹出这个空值
            lua_pop(L, 1);
            break;
        }
        ++count;
    } while (1);
    return count;
}
```