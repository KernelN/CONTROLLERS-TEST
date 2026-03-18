using System.Collections.Generic;
using UnityEngine;

namespace Universal
{
    public class UpdateManager : Singleton<UpdateManager>
    {
        List<Group> scaledGroups;
        Dictionary<float, Group> groupsByTime;
        Dictionary<int, Group> groupsByTicks;
        List<Group> unscaledGroups;
        Dictionary<float, Group> groupsByUTime;
        Dictionary<int, Group> groupsByUTicks;
        internal override void Awake()
        {
            base.Awake();
            if(this != inst) return;

            scaledGroups = new List<Group>();
            groupsByTime = new Dictionary<float, Group>();
            groupsByTicks = new Dictionary<int, Group>();
            unscaledGroups = new List<Group>();
            groupsByUTime = new Dictionary<float, Group>();
            groupsByUTicks = new Dictionary<int, Group>();
        }
        void Update()
        {
            float dt = Time.deltaTime;
            for (int i = 0; i < scaledGroups.Count; i++)
                scaledGroups[i].Update(dt);

            float udt = Time.unscaledDeltaTime;
            for(int i = 0; i < unscaledGroups.Count; i++)
                unscaledGroups[i].Update(udt);
        }
        void LateUpdate()
        {
            for (int i = 0; i < scaledGroups.Count; i++)
                scaledGroups[i].LateUpdate();
            
            for(int i = 0; i < unscaledGroups.Count; i++)
                unscaledGroups[i].LateUpdate();
        }

        void Suscribe(List<Group> groups, Dictionary<float, Group> groupsDict,
                        float time, System.Action action, bool isLate = false)
        {
            if(groupsDict.TryGetValue(time, out Group group))
                group.Add(action, isLate);
            else
            {
                Group newGroup = new Group(new TimeCounter(time));
                newGroup.Add(action, isLate);
                groupsDict.Add(time, newGroup);
                groups.Add(newGroup);
            }
        }
        void Remove(List<Group> groups, Dictionary<float, Group> groupsDict,
            float time, System.Action action, bool isLate = false)
        {
            if(!groupsDict.TryGetValue(time, out Group group)) return;

            if (group.Remove(action, isLate))
            {
                groupsDict.Remove(time);
                groups.Remove(group);
            }
        }
        void Suscribe(List<Group> groups, Dictionary<int, Group> groupsDict,
            int ticks, System.Action action, bool isScaled, bool isLate = false)
        {
            if(groupsDict.TryGetValue(ticks, out Group group))
                group.Add(action, isLate);
            else
            {
                Group newGroup = new Group(new TickCounter((ushort)ticks, isScaled));
                newGroup.Add(action, isLate);
                groupsDict.Add(ticks, newGroup);
                groups.Add(newGroup);
            }
        }
        void Remove(List<Group> groups, Dictionary<int, Group> groupsDict,
            int ticks, System.Action action, bool isLate = false)
        {
            if(!groupsDict.TryGetValue(ticks, out Group group)) return;

            if (group.Remove(action, isLate))
            {
                groupsDict.Remove(ticks);
                groups.Remove(group);
            }
        }
        public void SuscribeToScaled(float time, System.Action action) 
            => Suscribe(scaledGroups, groupsByTime, time, action);
        public void SuscribeToScaled(int ticks, System.Action action) 
            => Suscribe(scaledGroups, groupsByTicks, ticks, action, true);
        public void RemoveFromScaled(float time, System.Action action) 
            => Remove(scaledGroups, groupsByTime, time, action);
        public void RemoveFromScaled(int ticks, System.Action action) 
            => Remove(scaledGroups, groupsByTicks, ticks, action);
        public void SuscribeToUnscaled(float time, System.Action action)
            => Suscribe(unscaledGroups, groupsByUTime, time, action);
        public void SuscribeToUnscaled(int ticks, System.Action action)
            => Suscribe(unscaledGroups, groupsByUTicks, ticks, action, false);
        public void RemoveFromUnscaled(float time, System.Action action)
            => Remove(unscaledGroups, groupsByUTime, time, action);
        public void RemoveFromUnscaled(int ticks, System.Action action)
            => Remove(unscaledGroups, groupsByUTicks, ticks, action);
        public void SuscribeToLateScaled(float time, System.Action action)
            => Suscribe(scaledGroups, groupsByTime, time, action, true);
        public void SuscribeToLateScaled(int ticks, System.Action action)
            => Suscribe(scaledGroups, groupsByTicks, ticks, action, true, true);
        public void RemoveFromLateScaled(float time, System.Action action)
            => Remove(scaledGroups, groupsByTime, time, action, true);
        public void RemoveFromLateScaled(int ticks, System.Action action)
            => Remove(scaledGroups, groupsByTicks, ticks, action, true);
        public void SuscribeToLateUnscaled(float time, System.Action action)
            => Suscribe(unscaledGroups, groupsByUTime, time, action, true);
        public void SuscribeToLateUnscaled(int ticks, System.Action action)
            => Suscribe(unscaledGroups, groupsByUTicks, ticks, action, false, true);
        public void RemoveFromLateUnscaled(float time, System.Action action)
            => Remove(unscaledGroups, groupsByUTime, time, action, true);
        public void RemoveFromLateUnscaled(int ticks, System.Action action)
            => Remove(unscaledGroups, groupsByUTicks, ticks, action, true);
    }

    class Group
    {
        Counter counter;
        public List<System.Action> actions;
        public List<System.Action> lateActions;
        bool tickReady;

        public Group(Counter counter)
        {
            this.counter = counter;
            actions = new List<System.Action>();
            lateActions = new List<System.Action>();
        }
        
        public void Update(float dt)
        {
            if (counter.Update(dt))
            {
                tickReady = true;
                for (int i = 0; i < actions.Count; i++) 
                    actions[i]();
            }
        }
        public void LateUpdate()
        {
            if (tickReady)
            {
                counter.RenewCycle();
                for (int i = 0; i < lateActions.Count; i++)
                    lateActions[i]();
            }
        }
        public void Add(System.Action action, bool isLate)
        {
            if(isLate)
                lateActions.Add(action);
            else 
                actions.Add(action);
        }
        /// Returns true if empty
        public bool Remove(System.Action action, bool isLate)
        {
            if(isLate)
                lateActions.Remove(action);
            else 
                actions.Remove(action);
            
            return lateActions.Count == 0 && actions.Count == 0;
        }
    }

    abstract class Counter
    {
        public abstract bool Update(float dt);
        ///Resets cycle, but ignores overtime
        public abstract void ResetToZero();
        ///Resets cycle, taking into account overtime
        public abstract void RenewCycle();

        public abstract float GetDeltaTime();
    }

    class TimeCounter : Counter
    {
        float time, timer;

        public TimeCounter(float time)
        {
            this.time = time;
            timer = 0;
        }
        public override bool Update(float dt)
        {
            timer += dt;
            return timer > time;
        }
        public override void ResetToZero()
        {
            timer = 0;
        }
        public override void RenewCycle()
        {
            timer %= time;
        }
        public override float GetDeltaTime()
        {
            return timer;
        }
    }

    class TickCounter : Counter
    {
        ushort max, counter;
        float lastTimeStamp;
        bool useScaled;

        public TickCounter(ushort max, bool useScaled)
        {
            this.max = max;
            counter = 0;
            this.useScaled = useScaled;
            lastTimeStamp = useScaled ? Time.time : Time.unscaledTime;
        }
        public override bool Update(float dt)
        {
            counter++;
            return counter >= max;
        }
        public override void ResetToZero()
        {
            counter = 0;
            lastTimeStamp = useScaled ? Time.time : Time.unscaledTime;
        }
        public override void RenewCycle()
        {
            counter %= max;
            lastTimeStamp = useScaled ? Time.time : Time.unscaledTime;
        }
        public override float GetDeltaTime()
        {
            return useScaled ? Time.time : Time.unscaledTime - lastTimeStamp;
        }
    }
}