using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // 需要UI组件来显示

public class ClassScheduleManager : MonoBehaviour
{
    public static ClassScheduleManager Instance;

    // 可配置的课程和时间槽
    public string[] availableClasses = { "数学", "语文", "英语", "物理", "化学", "体育", "自习" };
    public string[] timeSlots = { "早自习", "第一节课", "第二节课", "课间操", "第三节课", "第四节课", "午休", "第五节课", "第六节课", "课外活动", "晚自习" };

    // 特殊事件标识
    public const string SPECIAL_EVENT_BREAK = "课间操";
    public const string SPECIAL_EVENT_LUNCH = "午休";
    public const string SPECIAL_EVENT_AFTER_SCHOOL = "课外活动";
    public const string SPECIAL_EVENT_NIGHT = "晚自习";

    // 存储一周的课表，外层List是每天，内层List是那天的课程事件
    public List<List<ClassEvent>> weeklySchedule = new List<List<ClassEvent>>();

    // 当前游戏时间状态
    [Header("当前状态")]
    public int currentDay = 1; // 第几天 (1-7)
    public int currentTimeSlot = 0; // 当前时间槽索引
    public ClassEvent currentEvent;

    // 事件：当时间推进时触发，其他脚本可以监听这个事件
    public static event Action<ClassEvent> OnPeriodChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 通常希望课表管理器持续存在
            GenerateWeeklySchedule();
            AdvanceToPeriod(0, 0); // 从第一天第一个时段开始
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ==================== 核心：生成随机课表 ====================
    void GenerateWeeklySchedule()
    {
        weeklySchedule.Clear();
        System.Random rand = new System.Random();

        for (int day = 0; day < 7; day++) // 生成一周七天
        {
            List<ClassEvent> dailySchedule = new List<ClassEvent>();
            List<string> classesToday = new List<string>(availableClasses);

            // 简单洗牌，打乱课程顺序
            Shuffle(classesToday, rand);

            int classIndex = 0;
            foreach (string slot in timeSlots)
            {
                ClassEvent newEvent = new ClassEvent();
                newEvent.timeSlotName = slot;

                // 处理特殊时段
                if (slot == SPECIAL_EVENT_BREAK || slot == SPECIAL_EVENT_LUNCH ||
                    slot == SPECIAL_EVENT_AFTER_SCHOOL || slot == SPECIAL_EVENT_NIGHT)
                {
                    newEvent.className = slot;
                    newEvent.isSpecial = true;
                }
                else
                {
                    // 分配常规课程，循环使用洗牌后的列表
                    newEvent.className = classesToday[classIndex % classesToday.Count];
                    newEvent.isSpecial = false;
                    classIndex++;
                }
                dailySchedule.Add(newEvent);
            }
            weeklySchedule.Add(dailySchedule);
        }
        Debug.Log("一周课表生成完毕！");
    }

    // 洗牌算法
    void Shuffle<T>(List<T> list, System.Random rand)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rand.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    // ==================== 时间推进与控制 ====================
    public void AdvanceToNextPeriod()
    {
        currentTimeSlot++;
        if (currentTimeSlot >= timeSlots.Length)
        {
            currentTimeSlot = 0;
            currentDay++;
            if (currentDay > 7) currentDay = 1; // 新的一周开始
        }
        UpdateCurrentEvent();
    }

    void AdvanceToPeriod(int day, int timeSlot)
    {
        currentDay = day + 1; // 转为1-based显示
        currentTimeSlot = timeSlot;
        UpdateCurrentEvent();
    }

    void UpdateCurrentEvent()
    {
        if (currentDay <= weeklySchedule.Count && currentTimeSlot < timeSlots.Length)
        {
            currentEvent = weeklySchedule[currentDay - 1][currentTimeSlot];
            Debug.Log($"时间推进：第{currentDay}天 - {currentEvent.timeSlotName} - {currentEvent.className}");

            // 触发事件，通知其他系统
            OnPeriodChanged?.Invoke(currentEvent);

            // 根据事件类型触发不同行为
            TriggerEventActions(currentEvent);
        }
    }

    // ==================== 事件触发 ====================
    void TriggerEventActions(ClassEvent evt)
    {
        if (evt.isSpecial)
        {
            switch (evt.timeSlotName)
            {
                case SPECIAL_EVENT_BREAK:
                    // 触发课间操事件，可以在这里播放广播体操音乐
                    Debug.Log("触发事件：课间操时间到了！");
                    // AudioManager.Instance.PlayMorningExerciseMusic();
                    break;
                case SPECIAL_EVENT_LUNCH:
                    Debug.Log("触发事件：午休时间，可以去食堂。");
                    break;
                case SPECIAL_EVENT_AFTER_SCHOOL:
                    Debug.Log("触发事件：课外活动，自由时间。");
                    break;
                case SPECIAL_EVENT_NIGHT:
                    Debug.Log("触发事件：晚自习开始。");
                    break;
            }
        }
        else
        {
            // 这里是正式课程，可以触发上课逻辑
            Debug.Log($"上课：《{evt.className}》");
            // 例如：调用 TeacherManager.StartClass(evt.className);
        }
    }

    // ==================== 工具方法 ====================
    // 获取今天完整的课表（用于UI显示）
    public List<ClassEvent> GetTodaySchedule()
    {
        if (currentDay - 1 < weeklySchedule.Count)
            return weeklySchedule[currentDay - 1];
        return new List<ClassEvent>();
    }

    // 获取特定一天的课表
    public List<ClassEvent> GetDaySchedule(int dayIndex)
    {
        if (dayIndex >= 0 && dayIndex < weeklySchedule.Count)
            return weeklySchedule[dayIndex];
        return new List<ClassEvent>();
    }

    // 手动测试用：在Unity编辑器里按T推进时间
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            AdvanceToNextPeriod();
        }
    }
}

// ==================== 课程事件数据结构 ====================
[System.Serializable]
public class ClassEvent
{
    public string timeSlotName; // 如“第一节课”
    public string className;    // 如“数学”或“课间操”
    public bool isSpecial;     // 是否为特殊事件
    // 可以扩展：教室位置、老师、所需物品等
}