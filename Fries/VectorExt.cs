using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace Fries {
    public static class VectorExt {
        /// <summary>
        /// Turn the given Vector3 into a Vector2 with v3's x and x value;
        /// </summary>
        public static Vector2 xx(this Vector3 vector3) => new(vector3.x, vector3.x);
        /// <summary>
        /// Turn the given Vector3 into a Vector2 with v3's x and y value;
        /// </summary>
        public static Vector2 xy(this Vector3 vector3) => new(vector3.x, vector3.y);
        /// <summary>
        /// Turn the given Vector3 into a Vector2 with v3's x and z value;
        /// </summary>
        public static Vector2 xz(this Vector3 vector3) => new(vector3.x, vector3.z);
        /// <summary>
        /// Turn the given Vector3 into a Vector2 with v3's y and x value;
        /// </summary>
        public static Vector2 yx(this Vector3 vector3) => new(vector3.y, vector3.x);
        /// <summary>
        /// Turn the given Vector3 into a Vector2 with v3's y and y value;
        /// </summary>
        public static Vector2 yy(this Vector3 vector3) => new(vector3.y, vector3.y);
        /// <summary>
        /// Turn the given Vector3 into a Vector2 with v3's y and z value;
        /// </summary>
        public static Vector2 yz(this Vector3 vector3) => new(vector3.y, vector3.z);
        /// <summary>
        /// Turn the given Vector3 into a Vector2 with v3's z and x value;
        /// </summary>
        public static Vector2 zx(this Vector3 vector3) => new(vector3.z, vector3.x);
        /// <summary>
        /// Turn the given Vector3 into a Vector2 with v3's z and y value;
        /// </summary>
        public static Vector2 zy(this Vector3 vector3) => new(vector3.z, vector3.y);
        /// <summary>
        /// Turn the given Vector3 into a Vector2 with v3's z and z value;
        /// </summary>
        public static Vector2 zz(this Vector3 vector3) => new(vector3.z, vector3.z);
        /// <summary>
        /// Turn the given Vector3 into a Vector2 with v3's input X and x value;
        /// </summary>
        public static Vector2 _x(this Vector3 vector3, float x) => new(x, vector3.x);
        /// <summary>
        /// Turn the given Vector3 into a Vector2 with v3's input X and y value;
        /// </summary>
        public static Vector2 _y(this Vector3 vector3, float x) => new(x, vector3.y);
        /// <summary>
        /// Turn the given Vector3 into a Vector2 with v3's input X and z value;
        /// </summary>
        public static Vector2 _z(this Vector3 vector3, float x) => new(x, vector3.z);
        /// <summary>
        /// Turn the given Vector3 into a Vector2 with v3's x and input Y value;
        /// </summary>
        public static Vector2 x_(this Vector3 vector3, float y) => new(vector3.x, y);
        /// <summary>
        /// Turn the given Vector3 into a Vector2 with v3's y and input Y value;
        /// </summary>
        public static Vector2 y_(this Vector3 vector3, float y) => new(vector3.y, y);
        /// <summary>
        /// Turn the given Vector3 into a Vector2 with v3's z and input Y value;
        /// </summary>
        public static Vector2 z_(this Vector3 vector3, float y) => new(vector3.z, y);
        /// <summary>
        /// Turn the given Vector3 into a Vector2 with input X and input Y value;
        /// </summary>
        public static Vector2 __(this Vector3 _, float x, float y) => new(x, y);
        
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's x, x and x value;
        /// </summary>
        public static Vector3 xxx(this Vector3 vector3) => new(vector3.x, vector3.x, vector3.x);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's x, x and y value;
        /// </summary>
        public static Vector3 xxy(this Vector3 vector3) => new(vector3.x, vector3.x, vector3.y);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's x, x and z value;
        /// </summary>
        public static Vector3 xxz(this Vector3 vector3) => new(vector3.x, vector3.x, vector3.z);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's x, y and x value;
        /// </summary>
        public static Vector3 xyx(this Vector3 vector3) => new(vector3.x, vector3.y, vector3.x);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's x, y and y value;
        /// </summary>
        public static Vector3 xyy(this Vector3 vector3) => new(vector3.x, vector3.y, vector3.y);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's x, y and z value;
        /// </summary>
        public static Vector3 xyz(this Vector3 vector3) => new(vector3.x, vector3.y, vector3.z);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's x, z and x value;
        /// </summary>
        public static Vector3 xzx(this Vector3 vector3) => new(vector3.x, vector3.z, vector3.x);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's x, z and y value;
        /// </summary>
        public static Vector3 xzy(this Vector3 vector3) => new(vector3.x, vector3.z, vector3.y);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's x, z and z value;
        /// </summary>
        public static Vector3 xzz(this Vector3 vector3) => new(vector3.x, vector3.z, vector3.z);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's y, x and x value;
        /// </summary>
        public static Vector3 yxx(this Vector3 vector3) => new(vector3.y, vector3.x, vector3.x);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's y, x and y value;
        /// </summary>
        public static Vector3 yxy(this Vector3 vector3) => new(vector3.y, vector3.x, vector3.y);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's y, x and z value;
        /// </summary>
        public static Vector3 yxz(this Vector3 vector3) => new(vector3.y, vector3.x, vector3.z);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's y, y and x value;
        /// </summary>
        public static Vector3 yyx(this Vector3 vector3) => new(vector3.y, vector3.y, vector3.x);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's y, y and y value;
        /// </summary>
        public static Vector3 yyy(this Vector3 vector3) => new(vector3.y, vector3.y, vector3.y);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's y, y and z value;
        /// </summary>
        public static Vector3 yyz(this Vector3 vector3) => new(vector3.y, vector3.y, vector3.z);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's y, z and x value;
        /// </summary>
        public static Vector3 yzx(this Vector3 vector3) => new(vector3.y, vector3.z, vector3.x);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's y, z and y value;
        /// </summary>
        public static Vector3 yzy(this Vector3 vector3) => new(vector3.y, vector3.z, vector3.y);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's y, z and z value;
        /// </summary>
        public static Vector3 yzz(this Vector3 vector3) => new(vector3.y, vector3.z, vector3.z);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's z, x and x value;
        /// </summary>
        public static Vector3 zxx(this Vector3 vector3) => new(vector3.z, vector3.x, vector3.x);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's z, x and y value;
        /// </summary>
        public static Vector3 zxy(this Vector3 vector3) => new(vector3.z, vector3.x, vector3.y);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's z, x and z value;
        /// </summary>
        public static Vector3 zxz(this Vector3 vector3) => new(vector3.z, vector3.x, vector3.z);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's z, y and x value;
        /// </summary>
        public static Vector3 zyx(this Vector3 vector3) => new(vector3.z, vector3.y, vector3.x);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's z, y and y value;
        /// </summary>
        public static Vector3 zyy(this Vector3 vector3) => new(vector3.z, vector3.y, vector3.y);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's z, y and z value;
        /// </summary>
        public static Vector3 zyz(this Vector3 vector3) => new(vector3.z, vector3.y, vector3.z);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's z, z and x value;
        /// </summary>
        public static Vector3 zzx(this Vector3 vector3) => new(vector3.z, vector3.z, vector3.x);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's z, z and y value;
        /// </summary>
        public static Vector3 zzy(this Vector3 vector3) => new(vector3.z, vector3.z, vector3.y);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's z, z and z value;
        /// </summary>
        public static Vector3 zzz(this Vector3 vector3) => new(vector3.z, vector3.z, vector3.z);
        
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's x and x values and input Z value;
        /// </summary>
        public static Vector3 xx_(this Vector3 vector3, float z) => new(vector3.x, vector3.x, z);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's x and y values and input Z value;
        /// </summary>
        public static Vector3 xy_(this Vector3 vector3, float z) => new(vector3.x, vector3.y, z);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's x and z values and input Z value;
        /// </summary>
        public static Vector3 xz_(this Vector3 vector3, float z) => new(vector3.x, vector3.z, z);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's x value, input Y value, and v3's x value;
        /// </summary>
        public static Vector3 x_x(this Vector3 vector3, float y) => new(vector3.x, y, vector3.x);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's x value, input Y value, and v3's y value;
        /// </summary>
        public static Vector3 x_y(this Vector3 vector3, float y) => new(vector3.x, y, vector3.y);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's x value, input Y value, and v3's z value;
        /// </summary>
        public static Vector3 x_z(this Vector3 vector3, float y) => new(vector3.x, y, vector3.z);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's x value and input Y and Z values;
        /// </summary>
        public static Vector3 x__(this Vector3 vector3, float y, float z) => new(vector3.x, y, z);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's y and x values and input Z value;
        /// </summary>
        public static Vector3 yx_(this Vector3 vector3, float z) => new(vector3.y, vector3.x, z);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's y and y values and input Z value;
        /// </summary>
        public static Vector3 yy_(this Vector3 vector3, float z) => new(vector3.y, vector3.y, z);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's y and z values and input Z value;
        /// </summary>
        public static Vector3 yz_(this Vector3 vector3, float z) => new(vector3.y, vector3.z, z);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's y value, input Y value, and v3's x value;
        /// </summary>
        public static Vector3 y_x(this Vector3 vector3, float y) => new(vector3.y, y, vector3.x);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's y value, input Y value, and v3's y value;
        /// </summary>
        public static Vector3 y_y(this Vector3 vector3, float y) => new(vector3.y, y, vector3.y);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's y value, input Y value, and v3's z value;
        /// </summary>
        public static Vector3 y_z(this Vector3 vector3, float y) => new(vector3.y, y, vector3.z);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's y value and input Y and Z values;
        /// </summary>
        public static Vector3 y__(this Vector3 vector3, float y, float z) => new(vector3.y, y, z);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's z and x values and input Z value;
        /// </summary>
        public static Vector3 zx_(this Vector3 vector3, float z) => new(vector3.z, vector3.x, z);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's z and y values and input Z value;
        /// </summary>
        public static Vector3 zy_(this Vector3 vector3, float z) => new(vector3.z, vector3.y, z);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's z and z values and input Z value;
        /// </summary>
        public static Vector3 zz_(this Vector3 vector3, float z) => new(vector3.z, vector3.z, z);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's z value, input Y value, and v3's x value;
        /// </summary>
        public static Vector3 z_x(this Vector3 vector3, float y) => new(vector3.z, y, vector3.x);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's z value, input Y value, and v3's y value;
        /// </summary>
        public static Vector3 z_y(this Vector3 vector3, float y) => new(vector3.z, y, vector3.y);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's z value, input Y value, and v3's z value;
        /// </summary>
        public static Vector3 z_z(this Vector3 vector3, float y) => new(vector3.z, y, vector3.z);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with v3's z value and input Y and Z values;
        /// </summary>
        public static Vector3 z__(this Vector3 vector3, float y, float z) => new(vector3.z, y, z);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with input X value and v3's x and x values;
        /// </summary>
        public static Vector3 _xx(this Vector3 vector3, float x) => new(x, vector3.x, vector3.x);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with input X value and v3's x and y values;
        /// </summary>
        public static Vector3 _xy(this Vector3 vector3, float x) => new(x, vector3.x, vector3.y);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with input X value and v3's x and z values;
        /// </summary>
        public static Vector3 _xz(this Vector3 vector3, float x) => new(x, vector3.x, vector3.z);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with input X value, v3's x value, and input Z value;
        /// </summary>
        public static Vector3 _x_(this Vector3 vector3, float x, float z) => new(x, vector3.x, z);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with input X value and v3's y and x values;
        /// </summary>
        public static Vector3 _yx(this Vector3 vector3, float x) => new(x, vector3.y, vector3.x);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with input X value and v3's y and y values;
        /// </summary>
        public static Vector3 _yy(this Vector3 vector3, float x) => new(x, vector3.y, vector3.y);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with input X value and v3's y and z values;
        /// </summary>
        public static Vector3 _yz(this Vector3 vector3, float x) => new(x, vector3.y, vector3.z);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with input X value, v3's y value, and input Z value;
        /// </summary>
        public static Vector3 _y_(this Vector3 vector3, float x, float z) => new(x, vector3.y, z);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with input X value and v3's z and x values;
        /// </summary>
        public static Vector3 _zx(this Vector3 vector3, float x) => new(x, vector3.z, vector3.x);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with input X value and v3's z and y values;
        /// </summary>
        public static Vector3 _zy(this Vector3 vector3, float x) => new(x, vector3.z, vector3.y);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with input X value and v3's z and z values;
        /// </summary>
        public static Vector3 _zz(this Vector3 vector3, float x) => new(x, vector3.z, vector3.z);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with input X value, v3's z value, and input Z value;
        /// </summary>
        public static Vector3 _z_(this Vector3 vector3, float x, float z) => new(x, vector3.z, z);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with input X and Y values, and v3's x value;
        /// </summary>
        public static Vector3 __x(this Vector3 vector3, float x, float y) => new(x, y, vector3.x);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with input X and Y values, and v3's y value;
        /// </summary>
        public static Vector3 __y(this Vector3 vector3, float x, float y) => new(x, y, vector3.y);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with input X and Y values, and v3's z value;
        /// </summary>
        public static Vector3 __z(this Vector3 vector3, float x, float y) => new(x, y, vector3.z);
        /// <summary>
        /// Turn the given Vector3 into a Vector3 with input X, Y, and Z values;
        /// </summary>
        public static Vector3 ___(this Vector3 _, float x, float y, float z) => new(x, y, z); 

        /// <summary>
        /// Turn the given Vector2 into a Vector2 with v2's x and x value;
        /// </summary>
        public static Vector2 xx(this Vector2 vector2) => new(vector2.x, vector2.x);
        /// <summary>
        /// Turn the given Vector2 into a Vector2 with v23's x and y value;
        /// </summary>
        public static Vector2 xy(this Vector2 vector2) => new(vector2.x, vector2.y);
        /// <summary>
        /// Turn the given Vector2 into a Vector2 with v2's y and x value;
        /// </summary>
        public static Vector2 yx(this Vector2 vector2) => new(vector2.y, vector2.x);
        /// <summary>
        /// Turn the given Vector2 into a Vector2 with v2's y and y value;
        /// </summary>
        public static Vector2 yy(this Vector2 vector2) => new(vector2.y, vector2.y);
        /// <summary>
        /// Turn the given Vector2 into a Vector2 with v2's input X and x value;
        /// </summary>
        public static Vector2 _x(this Vector2 vector2, float x) => new(x, vector2.x);
        /// <summary>
        /// Turn the given Vector2 into a Vector2 with v2's input X and y value;
        /// </summary>
        public static Vector2 _y(this Vector2 vector2, float x) => new(x, vector2.y);
        /// <summary>
        /// Turn the given Vector2 into a Vector2 with v2's x and input Y value;
        /// </summary>
        public static Vector2 x_(this Vector2 vector2, float y) => new(vector2.x, y);
        /// <summary>
        /// Turn the given Vector2 into a Vector2 with v2's y and input Y value;
        /// </summary>
        public static Vector2 y_(this Vector2 vector2, float y) => new(vector2.y, y);
        /// <summary>
        /// Turn the given Vector2 into a Vector2 with input X and input Y value;
        /// </summary>
        public static Vector2 __(this Vector2 _, float x, float y) => new(x, y);
        
        /// <summary>
        /// Turn the given Vector2 into a Vector3 with v2's x, x and x value;
        /// </summary>
        public static Vector3 xxx(this Vector2 vector2) => new(vector2.x, vector2.x, vector2.x);
        /// <summary>
        /// Turn the given Vector2 into a Vector3 with v2's x, x and y value;
        /// </summary>
        public static Vector3 xxy(this Vector2 vector2) => new(vector2.x, vector2.x, vector2.y);
        /// <summary>
        /// Turn the given Vector2 into a Vector3 with v2's x, y and x value;
        /// </summary>
        public static Vector3 xyx(this Vector2 vector2) => new(vector2.x, vector2.y, vector2.x);
        /// <summary>
        /// Turn the given Vector2 into a Vector3 with v2's x, y and y value;
        /// </summary>
        public static Vector3 xyy(this Vector2 vector2) => new(vector2.x, vector2.y, vector2.y);
        /// <summary>
        /// Turn the given Vector2 into a Vector3 with v2's y, x and x value;
        /// </summary>
        public static Vector3 yxx(this Vector2 vector2) => new(vector2.y, vector2.x, vector2.x);
        /// <summary>
        /// Turn the given Vector2 into a Vector3 with v2's y, x and y value;
        /// </summary>
        public static Vector3 yxy(this Vector2 vector2) => new(vector2.y, vector2.x, vector2.y);
        /// <summary>
        /// Turn the given Vector2 into a Vector3 with v2's y, y and x value;
        /// </summary>
        public static Vector3 yyx(this Vector2 vector2) => new(vector2.y, vector2.y, vector2.x);
        /// <summary>
        /// Turn the given Vector2 into a Vector3 with v2's y, y and y value;
        /// </summary>
        public static Vector3 yyy(this Vector2 vector2) => new(vector2.y, vector2.y, vector2.y);
        
        /// <summary>
        /// Turn the given Vector2 into a Vector3 with v2's x and x values and input Z value;
        /// </summary>
        public static Vector3 xx_(this Vector2 vector2, float z) => new(vector2.x, vector2.x, z);
        /// <summary>
        /// Turn the given Vector2 into a Vector3 with v2's x and y values and input Z value;
        /// </summary>
        public static Vector3 xy_(this Vector2 vector2, float z) => new(vector2.x, vector2.y, z);
        /// <summary>
        /// Turn the given Vector2 into a Vector3 with v2's x value, input Y value, and v2's x value;
        /// </summary>
        public static Vector3 x_x(this Vector2 vector2, float y) => new(vector2.x, y, vector2.x);
        /// <summary>
        /// Turn the given Vector2 into a Vector3 with v2's x value, input Y value, and v2's y value;
        /// </summary>
        public static Vector3 x_y(this Vector2 vector2, float y) => new(vector2.x, y, vector2.y);
        /// <summary>
        /// Turn the given Vector2 into a Vector3 with v2's x value and input Y and Z values;
        /// </summary>
        public static Vector3 x__(this Vector2 vector2, float y, float z) => new(vector2.x, y, z);
        /// <summary>
        /// Turn the given Vector2 into a Vector3 with v2's y and x values and input Z value;
        /// </summary>
        public static Vector3 yx_(this Vector2 vector2, float z) => new(vector2.y, vector2.x, z);
        /// <summary>
        /// Turn the given Vector2 into a Vector3 with v2's y and y values and input Z value;
        /// </summary>
        public static Vector3 yy_(this Vector2 vector2, float z) => new(vector2.y, vector2.y, z);
        /// <summary>
        /// Turn the given Vector2 into a Vector3 with v2's y value, input Y value, and v2's x value;
        /// </summary>
        public static Vector3 y_x(this Vector2 vector2, float y) => new(vector2.y, y, vector2.x);
        /// <summary>
        /// Turn the given Vector2 into a Vector3 with v2's y value, input Y value, and v2's y value;
        /// </summary>
        public static Vector3 y_y(this Vector2 vector2, float y) => new(vector2.y, y, vector2.y);
        /// <summary>
        /// Turn the given Vector2 into a Vector3 with v2's y value and input Y and Z values;
        /// </summary>
        public static Vector3 y__(this Vector2 vector2, float y, float z) => new(vector2.y, y, z);
        /// <summary>
        /// Turn the given Vector2 into a Vector3 with input X value and v2's x and x values;
        /// </summary>
        public static Vector3 _xx(this Vector2 vector2, float x) => new(x, vector2.x, vector2.x);
        /// <summary>
        /// Turn the given Vector2 into a Vector3 with input X value and v2's x and y values;
        /// </summary>
        public static Vector3 _xy(this Vector2 vector2, float x) => new(x, vector2.x, vector2.y);
        /// <summary>
        /// Turn the given Vector2 into a Vector3 with input X value, v2's x value, and input Z value;
        /// </summary>
        public static Vector3 _x_(this Vector2 vector2, float x, float z) => new(x, vector2.x, z);
        /// <summary>
        /// Turn the given Vector2 into a Vector3 with input X value and v2's y and x values;
        /// </summary>
        public static Vector3 _yx(this Vector2 vector2, float x) => new(x, vector2.y, vector2.x);
        /// <summary>
        /// Turn the given Vector2 into a Vector3 with input X value and v2's y and y values;
        /// </summary>
        public static Vector3 _yy(this Vector2 vector2, float x) => new(x, vector2.y, vector2.y);
        /// <summary>
        /// Turn the given Vector2 into a Vector3 with input X value, v2's y value, and input Z value;
        /// </summary>
        public static Vector3 _y_(this Vector2 vector2, float x, float z) => new(x, vector2.y, z);
        /// <summary>
        /// Turn the given Vector2 into a Vector3 with input X and Y values, and v2's x value;
        /// </summary>
        public static Vector3 __x(this Vector2 vector2, float x, float y) => new(x, y, vector2.x);
        /// <summary>
        /// Turn the given Vector2 into a Vector3 with input X and Y values, and v2's y value;
        /// </summary>
        public static Vector3 __y(this Vector2 vector2, float x, float y) => new(x, y, vector2.y);
        /// <summary>
        /// Turn the given Vector2 into a Vector3 with input X, Y, and Z values;
        /// </summary>
        public static Vector3 ___(this Vector2 _, float x, float y, float z) => new(x, y, z); 
        
        /// <summary>
        /// Turn the given float into a Vector3 with f, f, and f values;
        /// </summary>
        public static Vector3 fff(this float f) => new(f, f, f); 
        /// <summary>
        /// Turn the given float into a Vector2 with f, and f values;
        /// </summary>
        public static Vector2 ff(this float f) => new(f, f); 
        /// <summary>
        /// Turn the given float into a Vector3 with input X, input Y, and x values;
        /// </summary>
        public static Vector3 __f(this float f, float x, float y) => new(x, y, f); 
        /// <summary>
        /// Turn the given float into a Vector3 with input X, and f, f values;
        /// </summary>
        public static Vector3 _ff(this float f, float x) => new(x, f, f); 
        /// <summary>
        /// Turn the given float into a Vector3 with input X, and f, input Z values;
        /// </summary>
        public static Vector3 _f_(this float f, float x, float z) => new(x, f, z); 
        /// <summary>
        /// Turn the given float into a Vector3 with f, f, and input Z values;
        /// </summary>
        public static Vector3 ff_(this float f, float z) => new(f, f, z); 
        /// <summary>
        /// Turn the given float into a Vector3 with f, input Y, and f values;
        /// </summary>
        public static Vector3 f_f(this float f, float y) => new(f, y, f); 
        /// <summary>
        /// Turn the given float into a Vector3 with f, and input Y, input Z values;
        /// </summary>
        public static Vector3 f__(this float f, float y, float z) => new(f, y, z); 
        /// <summary>
        /// Turn the given float into a Vector3 with input X, and input Y, input Z values;
        /// </summary>
        public static Vector3 ___(this float _, float x, float y, float z) => new(x, y, z); 
        /// <summary>
        /// Turn the given float into a Vector2 with input X, and f values;
        /// </summary>
        public static Vector2 _f(this float f, float x) => new(x, f); 
        /// <summary>
        /// Turn the given float into a Vector2 with f, and input Y values;
        /// </summary>
        public static Vector2 f_(this float f, float y) => new(f, y); 
        /// <summary>
        /// Turn the given float into a Vector2 with input X, and input Y values;
        /// </summary>
        public static Vector2 __(this float _, float x, float y) => new(x, y); 

        public static Vector2 multiply(this Vector2 vector, Vector2 other) => new(vector.x * other.x, vector.y * other.y);
        public static Vector3 multiply(this Vector3 vector, Vector3 other) => new(vector.x * other.x, vector.y * other.y, vector.z * other.z);
        
        /// <summary>
        /// Turn the given World Pos Vector3 into a Screen Pos Vector3;
        /// The z dimension of the return value reflects how far away the World Pos is from the Camera.
        /// If camera is not specified, the method will use Camera.main instead.
        /// </summary>
        public static Vector3 toScreenPos(this Vector3 worldPos, Camera camera = null) {
            if (camera != null) return camera.WorldToScreenPoint(worldPos);
            Debug.Assert(Camera.main != null, "Camera.main != null");
            return Camera.main.WorldToScreenPoint(worldPos);
        }
        
        /// <summary>
        /// Turn the given Screen Pos Vector3 into a World Pos Vector3;
        /// The z dimension of the input value reflects how far away should the World Pos is from the Camera.
        /// If camera is not specified, the method will use Camera.main instead.
        /// </summary>
        public static Vector3 toWorldPos(this Vector3 screenPos, Camera camera = null) {
            if (camera != null) return camera.ScreenToWorldPoint(screenPos);
            Debug.Assert(Camera.main != null, "Camera.main != null");
            return Camera.main.ScreenToWorldPoint(screenPos);
        }
        
        /// <summary>
        /// Turn the given Screen Pos Vector2 into a World Pos Vector3;
        /// If dist2Camera not specified, the default value will be 0
        /// If camera is not specified, the method will use Camera.main instead.
        /// </summary>
        public static Vector3 toWorldPos(this Vector2 screenPos, float dist2Camera = 0, Camera camera = null) {
            Vector3 screenPos3 = new Vector3(screenPos.x, screenPos.y, dist2Camera);
            return toWorldPos(screenPos3, camera);
        }

        /// <summary>
        /// Turn Vector2 into Vector2Int
        /// </summary>
        public static Vector2Int toInt(this Vector2 vector2) {
            return new Vector2Int((int)vector2.x, (int)vector2.y);
        }
        
        /// <summary>
        /// Turn Vector3 into Vector3Int
        /// </summary>
        public static Vector3Int toInt(this Vector3 vector3) {
            return new Vector3Int((int)vector3.x, (int)vector3.y, (int)vector3.z);
        }
    }
}