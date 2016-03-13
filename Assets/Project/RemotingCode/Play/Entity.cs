﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Regulus.Collection;
using Regulus.CustomType;
using Regulus.Extension;
using Regulus.Project.ItIsNotAGame1.Data;
namespace Regulus.Project.ItIsNotAGame1.Game.Play
{
    public class Entity : IIndividual, IDevelopActor
        , IPlayerProperys
    {
        private readonly ENTITY _EntityType;
        private readonly string _Name;
        Rect _Bound;
        private readonly Polygon _Mesh;
        private readonly Guid _Id;

        private float _Speed;

        private readonly Regulus.Collection.DifferenceNoticer<IIndividual> _CollideTargets;
        public Regulus.Collection.DifferenceNoticer<IIndividual> CollideTargets { get { return _CollideTargets; } }

        private float _BaseView;

        private float _IlluminateView;

        private bool _Block;

        private float _DamageCount;

        private Vector2 _SkillOffsetVector;

        private float _BaseSpeed;

        private float _Strength;

        public event Action<Guid, float> InjuredEvent;

        private float _View
        {
            get
            {
                return _BaseView + _IlluminateView;
            }
        }
        

        private float _DetectionRange;

        private SkillData[] _Datas;


        public Inventory Bag { get; private set; }

        private Concierge _Concierge;

        

        public Entity(EntityData data, string name, ENTITY[] enter_types)
            : this(data, name, enter_types, new Inventory())
        {
            
        }

        
        public Entity(EntityData data) : this(data , "" , new ENTITY[0] , new Inventory() )
        {
            
            
        }


        public Entity(EntityData data, Inventory inventory) : this(data, "", new ENTITY[0], inventory)
        {

        }


        public Entity(EntityData data, string name) : this(data, name, new ENTITY[0], new Inventory())
        {
            
        }
        
        public Entity(Polygon mesh) 
        {
            _Mesh = mesh.Clone();
        }

        private Entity(EntityData data, string name , ENTITY[] enter_types ,Inventory inventory )
            : this()
        {
            _Name = name;
            Bag = inventory;
            _Mesh = data.Mesh.Clone();
            _Bound = this._BuildBound(this._Mesh);
            _Concierge = new Concierge(_Mesh, enter_types);
            _RotationMesh = data.CollisionRotation;
            _Name = "";
            _EntityType = data.Name;
            _DetectionRange = 1.0f + _Mesh.Points.ToRect().Width;

        }

        private Entity()
        {
            _Datas = Resource.Instance.SkillDatas;

            this._Id = Guid.NewGuid();
            this._BaseView = 15.0f;
            _DetectionRange = 1.0f ;


            _SignRoster = new SignRoster();
            _CollideTargets = new DifferenceNoticer<IIndividual>();

            _SkillOffsetVector = new Vector2();

            _BaseSpeed = 1.0f;
            _MaxHealth = 10f;

            Equipment = new Equipment(this);
            Equipment.AddEvent += _BroadcastEquipEvent;
            Equipment.RemoveEvent += _BroadcastEquipEvent;

            _Status = ACTOR_STATUS_TYPE.NORMAL_IDLE;
        }

        private void _BroadcastEquipEvent(Guid obj)
        {
            this._BroadcastEquipEvent();
        }

        private void _BroadcastEquipEvent()
        {
            var equipStatus = from item in this.Equipment.GetAll()
                              select new EquipStatus()
                              {
                                  Item = item.Name,
                                  Part = item.GetEquipPart()
                              };

            if (_EquipEvent != null)
            {
                _EquipEvent(equipStatus.ToArray());
            }
        }

        private void _BroadcastEquipEvent(Item obj)
        {

            _BroadcastEquipEvent();
        }


        ENTITY IVisible.EntityType { get { return _EntityType; } }

        Guid IVisible.Id { get { return _Id; } }

        public float Strength(float val)
        {
            
            _Strength += val;
            if (_Strength > 3.0f)
                _Strength = 3.0f;

            return _Strength;
        }

        public float Health(float val)
        {

            _Health += val;
            if (_Health > _MaxHealth)
                _Health = _MaxHealth;

            return _Health;
        }

        float IPlayerProperys.Strength {
            get { return _Strength; }
        }

        float IPlayerProperys.Health { get { return _Health; } }

        string IVisible.Name
        {
            get { return _Name; }
        }

        float IVisible.View { get { return _View; } }

        ACTOR_STATUS_TYPE IVisible.Status {
            get { return _Status;}
        }


        private event Action<EquipStatus[]> _EquipEvent;

        event Action<EquipStatus[]> IVisible.EquipEvent
        {
            add { this._EquipEvent += value; }
            remove { this._EquipEvent -= value; }
        }

        private event Action<VisibleStatus> _StatusEvent;

        event Action<VisibleStatus> IVisible.StatusEvent
        {
            add { this._StatusEvent += value; }
            remove { this._StatusEvent -= value; }
        }

        Vector2 IVisible.Position { get { return this._Mesh.Center; } }

        void IVisible.QueryStatus()
        {
            var status = new VisibleStatus()
            {
                Status = _Status,
                StartPosition = _Mesh.Center,
                Speed = _Speed,
                Direction = Direction,
                Trun = _Trun,
                SkillOffect = _SkillOffsetVector
            };

            this._StatusEvent.Invoke(status);        

            _BroadcastEquipEvent();
        }

        private event Action<string> _OnTalkMessageEvent;

        event Action<string> IVisible.TalkMessageEvent
        {
            add { this._OnTalkMessageEvent += value; }
            remove { this._OnTalkMessageEvent -= value; }
        }

        Rect IIndividual.Bounds
        {
            get
            {

                return this._Bound;
            }
        }

        private Rect _BuildBound(Polygon mesh)
        {

            return mesh.Points.ToRect();
        }

        Polygon IIndividual.Mesh
        {
            get { return this._Mesh; }
        }

        private Action _BoundsEvent;
        private float _Trun;

        private ACTOR_STATUS_TYPE _Status;

        private float _Health;

        private float _MaxHealth;

        private float _AidCount;

        private readonly bool _RotationMesh;

        private SignRoster _SignRoster;

        public Guid Id { get { return this._Id; } }

        public float Direction { get; private set; }

        public Equipment Equipment { get; set; }

        public float Speed { get { return _Speed; } }

        event Action IIndividual.BoundsEvent
        {
            add { this._BoundsEvent += value; }
            remove { this._BoundsEvent -= value; }
        }

        void IIndividual.SetPosition(Vector2 postion)
        {
            _SetPosition(postion.X, postion.Y);
        }
        void IIndividual.SetPosition(float x, float y)
        {
            _SetPosition(x, y);
        }

        private void _SetPosition(float x, float y)
        {
            var offset = new Vector2(x, y) - this._Mesh.Center;
            this._Mesh.Offset(offset);
            _UpdateBound();
        }

        private void _UpdateBound()
        {
            this._Bound = this._BuildBound(this._Mesh);
            if (this._BoundsEvent != null)
            {
                this._BoundsEvent.Invoke();
            }
        }

        Item[] IIndividual.Stolen(Guid id)
        {
            if (_SignRoster.Sign(id) == false)
            {
                return new Item[0];
            }
            if (_EntityType == ENTITY.DEBIRS)
            {
                _Status = ACTOR_STATUS_TYPE.CHEST_OPEN;
                _InvokeStatusEvent();
            }

            var item = Bag.GetAll().Shuffle().FirstOrDefault();
            if (item != null)
            {
                Bag.Remove(item.Id);
                return new[]
                {
                    item
                };
            }
            
            return new Item[0];
        }



        bool IIndividual.IsBlock()
        {
            return _Block;
        }

        Concierge IIndividual.GetConcierge()
        {
            return _Concierge;
        }

        

        void IIndividual.AttachHit(Guid target , HitForce force)
        {
            var damage = force.Damage;
            _DamageCount += damage;

            _AidCount += force.Aid;

            if (InjuredEvent != null)
                InjuredEvent(target , damage);
        }

        public void UpdatePosition(Vector2 velocity)
        {
            if (velocity.X == 0 && velocity.Y == 0)
                return;

            this._Mesh.Offset(velocity);
            this._Bound = this._BuildBound(this._Mesh);
            if (this._BoundsEvent != null)
                this._BoundsEvent.Invoke();


        }

        public void Stop()
        {
            this._Speed = 0.0f;
            _InvokeStatusEvent();
        }
        internal void Trun(float trun)
        {
            if (trun != _Trun)
            {
                this._Trun = trun;
                _InvokeStatusEvent();
            }

        }

        public void Back(float speed)
        {            
            if (speed != _Speed)
            {
                this._Speed = speed * _BaseSpeed;
                _InvokeStatusEvent();
            }
        }

        public void Move(float angle, float speed)
        {            
            if (speed != _Speed)
            {
                this._Speed = speed * _BaseSpeed;
                _AddRotation(angle);
                _InvokeStatusEvent();
            }
        }

        private void _AddDirection(float angle)
        {
            Direction = (this.Direction + angle) % 360;
            Direction += 360; 
            Direction %= 360; 
        }

        private void _InvokeStatusEvent()
        {

            if (_StatusEvent != null)
            {
                var status = new VisibleStatus()
                {
                    Status = _Status,
                    StartPosition = _Mesh.Center,
                    Speed = _Speed,
                    Direction = Direction,
                    Trun = _Trun,
                    SkillOffect = _SkillOffsetVector
                };
                this._StatusEvent.Invoke(status);                

            }
        }
        public Vector2 GetDirectionVector()
        {
            return this._ToVector(this.Direction);
        }

        public Vector2 GetVelocity(float delta_time)
        {            
                        
            var skill = _SkillOffsetVector * delta_time;
            return this._ToVector(this.Direction) * delta_time * this._Speed + skill ;
        }

        public void TrunDirection(float delta_time)
        {
            _AddRotation(_Trun * delta_time);
        }

        private Vector2 _ToVector(float angle)
        {
            var radians = angle * 0.0174532924;
            return new Vector2((float)Math.Cos(radians), (float)-Math.Sin(radians));
        }

        private Rect _BuildVidw()
        {
            var center = this._Mesh.Center;
            var hw = this._View / 2;
            var hh = this._View / 2;
            var rect = new Rect(center.X - hw, center.Y - hh, this._View, this._View);
            return rect;
        }


        public float GetDetectionRange()
        {
            return _DetectionRange;
        }
        public Polygon GetExploreBound()
        {
            var ext = new ExtendPolygon(_Mesh, _DetectionRange);
            return ext.Result;
        }

        public Rect GetView()
        {
            return _BuildVidw();
        }

        public void Normal()
        {
            _Status = ACTOR_STATUS_TYPE.NORMAL_IDLE;
            Stop();
        }

        public void Explore()
        {
            _Status = ACTOR_STATUS_TYPE.NORMAL_EXPLORE;
            _InvokeStatusEvent();
        }
        private ACTOR_STATUS_TYPE _GetWeaponIdle(ITEM_FEATURES features)
        {
            switch (features)
            {
                case ITEM_FEATURES.WEAPON_AXE:
                    return ACTOR_STATUS_TYPE.BATTLE_AXE_IDLE;
                case ITEM_FEATURES.WEAPON_TWOHANDSWORD:
                    return ACTOR_STATUS_TYPE.TWO_HAND_SWORD_IDLE;
                default:
                    return ACTOR_STATUS_TYPE.MELEE_IDLE;
            }
        }




        public void CastBegin(ACTOR_STATUS_TYPE id)
        {
            _Cast(id);

        }

        private void _Cast(ACTOR_STATUS_TYPE id)
        {
            _Speed = 0.0f;
            _Trun = 0.0f;
            _Status = id;
            _InvokeStatusEvent();
        }

        public void CastEnd(ACTOR_STATUS_TYPE id)
        {

        }

        public float HaveAid()
        {
            var values = _AidCount;
            _AidCount = 0;
            return values;
        }

        public float HaveDamage()
        {
            var values = _DamageCount;
            _DamageCount = 0;
            return values;
        }

        public void Damage()
        {
            _Speed = 0.0f;
            _Trun = 0.0f;
            _Status = ACTOR_STATUS_TYPE.DAMAGE1;
            _InvokeStatusEvent();

        }

        public SkillCaster GetDamagrCaster()
        {
            var data = _Datas.First((s) => s.Id == ACTOR_STATUS_TYPE.DAMAGE1);
            return new SkillCaster(data, new Determination(data.Lefts, data.Rights, data.Total, data.Begin, data.End));
        }

        public void SetBlock(bool set)
        {
            _Block = set;
        }

        public Vector2 GetPosition()
        {
            return _Mesh.Center;
        }

        private void _AddRotation(float degree)
        {
            _AddDirection(degree);
            if (_RotationMesh)
            {
                _Mesh.RotationByDegree(degree);
                _UpdateBound();
            }                
                
        }

        public void Talk(string message)
        {
            if (_OnTalkMessageEvent != null)
                _OnTalkMessageEvent(message);
        }

        public void Make()
        {
            _Speed = 0.0f;
            _Trun = 0.0f;
            _Status = ACTOR_STATUS_TYPE.MAKE;
            _InvokeStatusEvent();
        }

        public ItemFormula[] GetFormulas()
        {
            return Resource.Instance.Formulas;
        }


        public void SetCollisionTargets(IEnumerable<IIndividual> hitthetargets)
        {
            _CollideTargets.Set(hitthetargets);
        }

        void IIndividual.AddDirection(float dir)
        {
            _AddRotation(dir);
            _InvokeStatusEvent();
        }

        

        public ACTOR_STATUS_TYPE GetIdle()
        {
            return _Status;
        }

        public void SetEquipView(float view)
        {
            _IlluminateView = view;
        }

        void IDevelopActor.SetBaseView(float range)
        {
            _BaseView = range;
        }

        void IDevelopActor.SetSpeed(float speed)
        {
            _BaseSpeed = speed;
        }

        public SkillCaster GetBattleCaster()
        {

            ACTOR_STATUS_TYPE status = ACTOR_STATUS_TYPE.MELEE_IDLE;
            var item = this.Equipment.Find(EQUIP_PART.RIGHT_HAND);
            if (item!=null)
            {
                status = _GetWeaponIdle(item.GetPrototype().Features);
            }

            var data = _Datas.First((s) => s.Id == status);
            return new SkillCaster(data, new Determination(data.Lefts, data.Rights, data.Total, data.Begin, data.End));
        }

        public void SetSkillVelocity(float direction, float speed)
        {
            var dir = ((Direction + direction) + 360.0f) % 360.0f;
            _SkillOffsetVector = _ToVector(dir) * speed;
        }

        public void ClearOffset()
        {
            this._Speed = 0.0f;
            _SkillOffsetVector = new Vector2();            
            _InvokeStatusEvent();
        }

        public float GetStrength()
        {
            return Strength(0);
        }

        public IVisible GetVisible()
        {
            return this;
        }

        public ACTOR_STATUS_TYPE GetStatus()
        {
            return _Status;
        }

        public void Stun()
        {
            _Speed = 0.0f;
            _Trun = 0.0f;
            _Status = ACTOR_STATUS_TYPE.STUN;
            _InvokeStatusEvent();
        }

        public void ResetProperty()
        {
            _Health = _MaxHealth;
            _Strength = 3f;
            _DamageCount = 0f;
            _AidCount = 0f;
        }

        public void RecoveryStrength(float delta_time)
        {
            Strength(delta_time);
        }

        public void RecoveryHealth(float delta_time)
        {
            if (_Status == ACTOR_STATUS_TYPE.NORMAL_IDLE)
                Health(delta_time);            
        }

        public float GetViewLength()
        {
            return _View;
        }

        public void Aid()
        {
            _Speed = 0.0f;
            _Trun = 0.0f;
            _Status = ACTOR_STATUS_TYPE.AID;
            _InvokeStatusEvent();
        }
    }
}