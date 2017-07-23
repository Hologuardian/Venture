﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Assets.Engine.Src;
using Assets.Station.Src.Requests;

namespace Assets.Station.Src
{
    public abstract class VolatileModule : VolatileObject
    {
        // Properties
        // Ensure proper care is taken to considering which must be Mementos and which can be generic
        /// <summary>
        /// All modules have a name, this is the name of their table in the database, and their unique name in game data
        /// </summary>
        public string Name { get; protected set; }
        /// <summary>
        /// The nonvolatile representation of this module in Unity, this should be accessed carefully, as it will not be locked at anytime, and could be the source of race conditions, especially with Unity.
        /// If you want to do anything to this object you should queue a unity task to do just that, the next time Unity is able.
        /// </summary>
        public Module UnityObject { get; protected set; }
        /// <summary>
        /// All modules come in 1 of 5 sizes, these sizes are cubes with specific dimensions used in pathfinding, and in space partitioning.
        /// As well these sizes define what tier of station technology this module is.
        /// </summary>
        public Size Size { get; protected set; }
        /// <summary>
        /// The dimensions in cubes of the size specified with the this.size property.
        /// </summary>
        public Vector3 Dimensions { get; protected set; }
        /// <summary>
        /// The mass of this module, before any inventory.
        /// </summary>
        public float TrueMass { get; protected set; }
        /// <summary>
        /// The volume this module has for inventory.
        /// </summary>
        public float MaximumVolume { get; protected set; }
        /// <summary>
        /// The maximum pressurisation this module can maintain before suffering structural damage.
        /// </summary>
        public float MaximumPressurisation { get; protected set; }
        /// <summary>
        /// The percentage of incoming power that is lost in travel across this module.
        /// </summary>
        public float LineLoss { get; protected set; }

        // Mementos
        private Memento<float> mass;
        /// <summary>
        /// The current mass of this module, after accounting for inventory.
        /// </summary>
        public float Mass
        {
            get
            {
                return mass;
            }
            set
            {
                mass.Set(value);
            }
        }
        private Memento<float> volume;
        /// <summary>
        /// The current volume of this module remaining, after accounting for inventory.
        /// </summary>
        public float Volume
        {
            get
            {
                return volume;
            }
            set
            {
                volume.Set(value);
            }
        }
        private Memento<float> pressurisation;
        /// <summary>
        /// The current pressurisation of this module, in pascals.
        /// </summary>
        public float Pressurisation
        {
            get
            {
                return pressurisation;
            }
            set
            {
                pressurisation.Set(value);
            }
        }
        private Memento<float> energyProduction;
        /// <summary>
        /// The amount of energy this module produces per hour in watt hours (negative in cases of power consumption).
        /// </summary>
        public float EnergyProduction
        {
            get
            {
                return energyProduction;
            }
            set
            {
                energyProduction.Set(value);
            }
        }

        public VolatileModule(string name)
        {
            Name = name;

            mass = new Memento<float>(Name + ".mass");
            volume = new Memento<float>(Name + ".volume");
            pressurisation = new Memento<float>(Name + ".pressurisation");
            energyProduction = new Memento<float>(Name + ".energyproduction");
        }

        // Methods
        /// <summary>
        /// Update is a thread safe method which locks and calls OverridableUpdate
        /// As well Update performs any general module logic, such as:
        /// Distributing Electricity
        /// Distributing Events
        /// Distributing Resources, given input and output hardpoints
        /// </summary>
        public void Update()
        {
            lock (this)
            {
                // RESET ----------------------------------------------------------------------------------
                // Reset all parameters that reset every update
                EnergyProduction = 0;

                // REQUESTS -------------------------------------------------------------------------------
                // First, all update requests, as well as a variety of others come from the hardpoints, in this manner one module speaks to the others hardpoints locking them
                // Since each talks to the others, this allows for 2 way dialogue without causing either to lock the other with direct module to module talk.
                // To do that we need to get all the hardpoint requests from this object, and take all their requests.
                foreach (Hardpoint hardpoint in UnityObject.hardpoints)
                {
                    // This prevents any race conditions
                    lock (hardpoint.threaded)
                    {
                        // Take all the requests from the hardpoint
                        foreach (Request request in hardpoint.threaded.requests)
                        {
                            requests.Add(request);
                        }

                        // Clear the hardpoints requests
                        hardpoint.threaded.requests.Clear();
                    }
                }

                // Process all the requests
                foreach (Request request in requests)
                {
                    // Requests must be fast executing pieces of logic
                    request.Do(this);
                }

                // MODULE ---------------------------------------------------------------------------------
                OverridableUpdate();

                // SUBMODULES -----------------------------------------------------------------------------

                // HARDPOINTS -----------------------------------------------------------------------------
                // First calculate all output values for each hardpoint connection
                // Second send an update down every hardpoint with the necessary outputs
                foreach (Hardpoint hardpoint in UnityObject.hardpoints)
                {
                    // Each hardpoint connection queues UpdateModuleRequests on the other modules hardpoint
                    UpdateModuleRequest request = new UpdateModuleRequest();
                    // Calculate the output through each hardpoint
                    // Electricity
                    // If the connected module has higher EnergyProduction than this one then don't send power (you are probably recieving power from it anyways)
                    if (hardpoint.connection.module.threaded.EnergyProduction < EnergyProduction)
                        request.energyIn = Mathf.Max(EnergyProduction * LineLoss, 0);

                    // Distribution


                    // Events


                    // Queue hardpoint update, with the necessary inputs
                    hardpoint.connection.threaded.Request(request);
                }

                // LASTPASS ------------------------------------------------------------------------------
                // All the final updates to ensure that user data is correct
            }
        }

        /// <summary>
        /// This method is the overridable method used to give OverridableUpdate child classes actual logic, while gauranteeing thread safety, as Update locks this before calling OverridableUpdate
        /// </summary>
        public abstract void OverridableUpdate();
    }
}