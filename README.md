# 08-Fireworks

**Author:** Julie Vondráčková

## Overview
This task was moved from private repository for Master's studies application process. The outline was provided by our course teacher Josef Pelikán for course NPGR003 at Charles University, Prague in 2023

The "08-Fireworks" project is a simulation designed to generate and animate various types of firework effects using particle systems. It includes different types of particles, such as rockets and explosions, to create vibrant and dynamic firework displays.

## Particle Definition
The system defines an abstract base class called `Particle`, which encapsulates the common properties and behaviors of fireworks particles, such as position, color, size, and velocity. Derived classes, such as `RocketParticle` and `ExplosionParticle`, specialize this base class to represent specific firework components, each with unique behaviors and visual effects.

## Simulation Engine
The core of the project is the `Simulation` class, which manages the life cycle of all particles in the system. It is responsible for initializing the particle system, simulating particle dynamics over time, and handling the generation of new particles to replace those that have expired. The simulation supports various explosion patterns (e.g., sphere, cube) and dynamically adjusts particle properties to achieve realistic firework effects.

## Command-line Arguments
The project does not explicitly define command-line arguments. It is designed to be integrated into applications that can programmatically control and display the simulation.

## Input Data
The input data is provided in a .json file, which was part of the original solution example.

## Algorithm
Initialize the simulation with a predefined number of particles.
1. In each simulation step, it updates the positions of the particles based on their velocities and applies aging to reduce their size and fade their colors over time.
2. Remove particles that have reached the end of their life cycle.
3. Introduce new particles to replace expired ones, including launching new rockets and generating explosions at the end of the rockets' trajectories.
4. Dynamically adjust particle properties to simulate different explosion patterns and effects.

## Extra Features / Bonuses
Launching rockets by pressing the space bar.


Three different types of explosions: spherical, cubic, and "splash."


It's nicely colorful.


Rockets slow down over time.




## Use of AI
1. [help with explosion types](https://chat.openai.com/share/0e7d8966-418b-4ad5-96c8-4735d2665210)

2. [writing documentation :)](https://chat.openai.com/share/99b490be-2252-4571-8081-5a29199acc3a)
