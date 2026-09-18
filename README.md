# GOCHI: Dinner Is on You!

A 2D Unity game developed as my senior project for the
Simulation, Animation & Gaming program at Eastern Michigan University.

The game is inspired by a restaurant price-guessing challenge.
Players order food while trying to finish as close as possible to a
random target price.

## Demo



https://github.com/user-attachments/assets/6d0a2a00-8886-4f4b-b538-c542236d3fd7



## Screenshots

[Title Screen]<img width="971" height="347" alt="title-screen" src="https://github.com/user-attachments/assets/ab8ecca8-9957-44c3-b459-677e4c61671f" />


[Menu]<img width="1858" height="973" alt="Screenshot 2026-09-18 192607" src="https://github.com/user-attachments/assets/1a6d1c48-8aa9-4798-b634-593254121482" />

[Gameplay]<img width="843" height="448" alt="Screenshot 2026-09-18 192715" src="https://github.com/user-attachments/assets/5eac76a7-9b9f-456d-8a19-89cf2e1912ff" />


[Results]<img width="828" height="441" alt="Screenshot 2026-09-18 192804" src="https://github.com/user-attachments/assets/2750fb28-0ddd-4c99-9e2a-505b783a6f1f" />



## My Contribution

I designed and programmed the game in Unity.

My work included:

- Implementing the core ordering and price-target gameplay
- Creating the player interaction flow
- Programming menu selection and order logic
- Implementing randomized target prices
- Building game UI and dialogue sequences
- Implementing computer-controlled opponents
- Debugging gameplay and UI issues
- Testing and refining the game flow

## Tools & Technologies

- Unity 2022.3.49 LTS
- C#
- Visual Studio Code

## Core Gameplay

Players are given a target restaurant bill and choose food items from
the menu.

The objective is to finish as close as possible to the target amount.

The game includes:

- Randomized target prices
- Multiple menu items with different prices
- Player ordering
- Computer-controlled opponents
- Price guesses
- Final result comparison

A player who finishes exactly on the target achieves a **Pitari**.

A result within 500 yen of the target qualifies as a **Near Pin**.

## Game Flow

1. Start the game
2. View the tutorial
3. Receive a randomly generated target price
4. Make the first order
5. Continue ordering menu items
6. Compare the final totals
7. Determine who finished closest to the target

## Technical Challenges

### Managing UI State

One challenge was controlling when different Unity UI panels became
active.

Some objects could not start coroutines while inactive, so I revised
the order in which UI objects were activated before starting dialogue
and gameplay sequences.

### Debugging NullReference Errors

During development, I encountered NullReference errors involving
gameplay dialogue and UI references.

I traced the missing references and corrected the object setup and
activation sequence.

### Coordinating Game States

The project required multiple stages—including the tutorial, target
price reveal, first-order sequence, menu interaction, and results.

I organized the game flow so that each stage transitions to the next
at the correct time.

## What I Learned

This project gave me practical experience with:

- C# scripting in Unity
- Debugging
- UI programming
- Game-state management
- Randomized gameplay systems
- Building an interactive project from concept through completion

## About Me

I am a recent graduate of Eastern Michigan University's
Bachelor of Science in Simulation, Animation & Gaming program.

I am interested in entry-level opportunities involving:

- Unity development
- Simulation
- Real-time 3D
- Interactive applications
- Visualization
- Game development

LinkedIn:
https://www.linkedin.com/in/takuma-ono-317a97299/
