# GOCHI: Dinner Is on You!

A 2D Unity game developed as my senior project for the
Simulation, Animation & Gaming program at Eastern Michigan University.

The game is inspired by a restaurant price-guessing challenge.
Players order food while trying to finish as close as possible to a
random target price.

## Demo

[Add a 30–60 second gameplay video link here]

## Screenshots

![Title Screen](screenshots/title-screen.png)

![Menu](screenshots/menu.png)

![Gameplay](screenshots/gameplay.png)

![Results](screenshots/results.png)

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
