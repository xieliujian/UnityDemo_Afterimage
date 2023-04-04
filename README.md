# unity的残影效果

## 基础残影

![图标](https://github.com/xieliujian/UnityDemo_Afterimage/blob/main/Video/Afterimage.png?raw=true)

如下图参数所示，编辑属性面板，记得点击 
> 刷新不可见模型名字列表

![图标](https://github.com/xieliujian/UnityDemo_Afterimage/blob/main/Video/Afterimage2.png?raw=true)

## 帧方向残影

![图标](https://github.com/xieliujian/UnityDemo_Afterimage/blob/main/Video/FrameDirAfterimage.png?raw=true)

#### 参数编辑

![图标](https://github.com/xieliujian/UnityDemo_Afterimage/blob/main/Video/FrameDirAfterimage1.png?raw=true)

#### 残影资源

残影效果共用一个资源文件，这个资源需要隐藏，把Rendering Layer Mask 设置Nothing来隐藏资源模型

![图标](https://github.com/xieliujian/UnityDemo_Afterimage/blob/main/Video/FrameDirAfterimage2.png?raw=true)

#### 残影目标点

每一个帧方向残影需要一个目标，这个目标确定残影的方向和最终移动点

![图标](https://github.com/xieliujian/UnityDemo_Afterimage/blob/main/Video/FrameDirAfterimage3.png?raw=true)

如图所示，FrameDirAfterimage关联的Cube目标