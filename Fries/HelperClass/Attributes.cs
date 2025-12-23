using System;

namespace Fries.HelperClass {
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class ToHelperMethod : Attribute { }
}

// 功能描述
// HelperClass 模块的主要作用是
// 如果一个方法被打上 [HelperMethod] 标记，那么 Rider 将弹出一个 CodeFix 建议给我们
// 采纳这个建议后，CodeFix 会将目标方法所在的类变为 partial 类，并且创建新的 partial 对应类（叫做 类名.h.cs）。在该类中
// 新建 private class 类名_h 类，内部放置 internal 的该方法的静态版本

// 以下是具体案例：
/*
 ...
 private int x;
 private int y;
 [HelperMethod]
 private int doSomeMath() {
    return x+y;
 }
*/
// 以上代码会被转化为
/*
 ...
 private int x;
 private int y;
 private int doSomeMath() => Class_h.doSomeMath(x, y);
 
 private class Class_h {
  internal static int doSomeMath(int x, int y) {
   return x+y;
  }
 }
*/
// 简单来说，在生成方法时，方法会试图检测方法内部有哪些来自非局部的变量，并将这些变量在参数中声明
// 如果方法本身携带参数，比如说：
/*
 private int x;
 private int y;
 private int doSomeMath(int c) {
  return x+y+c;
 }
*/
// 方法会被转换为
/*
 ...
 private int doSomeMath(int c) => Class_h.doSomeMath(c, x, y);
 internal static int doSomeMath(int c, int x, int y) {
  return x+y+c;
 }
*/
// 如果方法调用了上下文中的静态属性，那么也传递该属性
/*
 private static int result;
 private int doSomeMath() {
  return result;
 }
 */
// 会变为
/*
 private int doSomeMath() => Class_h.doSomeMath(result);
 ...
 internal static int doSomeMath(int result) { return result; }
 */
// 但是如果该方法中调用了其他类公开的静态属性，则不需要额外的收集该值。例如
/*
 private int doSomeMath() {
  return AnotherClass.Result;
 }
 */
// 会变成
/*
 private int doSomeMath() => Class_h.doSomeMath();
 ...
 internal static int doSomeMath() { return AnotherClass.Result; }
 */
// 应该说，区分方式是 - 该值如果仅在当前上下文中存在，那么就把它变成参数传递
// 该值如果是此实例成员字段，那么就把它变成参数传递

// 接下来，对于 override 或者 new 方法，我们不做上述检查。如果它有 [HelperMethod] 注解，我们会爆出警告 - Override 与 new 方法不支持转换为 分离的帮助方法
// 同样，对于 virtual, abstract, 或者接口类中未实现的方法我们也不支持这些操作

// 如果该方法没有返回值，那么生成出的方法也没有返回值
// 如果该方法是静态方法，生成规则也不变

// 最后，如果方法原参数中包含 in/out/ref 参数，则不处理这样的方法。方法例如
/*
 private int doSomeMath(ref ParamsStruct p) { ... }
 不处理这样的方法。如果用户想要在高性能场景下优化，用户应该生成好方法后，手动修改两边的参数为引用参数
*/