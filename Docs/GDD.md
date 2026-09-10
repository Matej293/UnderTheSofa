<div class="titlepage">

**UNDER THE SOFA**

Game Design Document

------------------------------------------------------------------------

|                       |                           |
|:----------------------|:--------------------------|
| **Document Version:** | 0.1                       |
| **Project:**          | Under the Sofa            |
| **Developer:**        | Solo independent project  |
| **Target Platform:**  | Windows PC                |
| **Engine:**           | Unity 6.5                 |
| **Render Pipeline:**  | Universal Render Pipeline |
| **Date:**             | August 2026               |
| **Author:**           | Matej Spajić              |

This document will change as the game is tested and built.

</div>

# Revision History

| **Version** | **Date** | **Author** | **Status** | **Changes** |
|:---|:---|:---|:---|:---|
| 0.1 | Aug. 2026 | Developer | Draft | Initial game concept, mechanics, scope, art direction, and production plan. |

# Game Goals

## High Concept

*Under the Sofa* is a third-person driving and exploration game about a
tiny RC car inside a normal suburban house.

The house feels enormous because of the car’s size. A sofa becomes a
structure to climb. Books work as steps. A chair can form part of a
route to a kitchen counter. A cardboard box can become a jump without
needing to look like a videogame ramp.

The player drives through the house and backyard, collects rings,
reaches difficult places, finds shortcuts, and performs tricks along the
way.

There are no enemies and no combat. The car and the environment have to
carry the game on their own.

## Creative Reference

*Re-Volt* (1999) is the main gameplay and tone reference for *Under the
Sofa*.

The game takes inspiration from its fast RC car handling, exaggerated
physics, small vehicles moving through oversized everyday environments,
and late-1990s electronic soundtrack.

*Under the Sofa* is not built around conventional racing. The same style
of responsive RC movement is used for exploration, climbing, jumping,
collecting, and stunt play inside a connected house and backyard.

The goal is to capture the feeling of controlling a light, fast toy car
while building a different game around that movement.

## Short Pitch

> Drive a tiny RC car through a giant house and backyard. Use furniture,
> toys, books, boxes, and other household objects as ramps and platforms
> while collecting rings, finding hidden routes, and performing stunts.

## What the Player Should Want to Do

The player should look at something high up and want to reach it.

The kitchen counter is a good example. At first it looks far above the
car. Then the player notices a chair, a cardboard box beside it, and
perhaps a book lying at an angle nearby. Those objects start to look
like a route.

That kind of thinking should happen throughout the game.

Driving across the floor is still fun, but the floor is mostly where the
player starts. The better parts of each room are above, below, behind,
or inside things.

## Main Design Rules

Movement has to feel good before the rest of the game matters. Driving
around an empty test room should already be enjoyable.

Rooms should be built for the car. They need to look like believable
household spaces, but furniture placement can bend reality when the game
needs a better route or jump.

The game should reuse objects often. The same books, boxes, chairs,
toys, and cushions can appear in different rooms if their placement
changes how the player moves through the area.

The visual style also needs discipline. Thirty assets that fit together
are better than hundreds of free assets from unrelated packs.

## Player Experience

The game should feel relaxed when the player is exploring and more
demanding when they decide to take a difficult route.

Missing a jump should not cost much. The player can try again within
seconds.

A lot of the fun should come from small personal goals. Reaching the top
of the sofa for the first time matters even if the game never explicitly
tells the player to do it.

Some locations will have formal rewards. Others can exist because
getting there feels good.

## Target Audience

The game is aimed at players who like arcade driving, 3D platforming,
collectathons, movement challenges, and late-1990s 3D visuals.

The game should stay easy to understand. A new player should know how to
drive within a minute or two.

Mastering jumps and routes can take longer.

## Platform and Scope

The first release targets Windows PC.

Other platforms are outside the current scope.

The project is being made with a near-zero asset budget, so paid assets
are not part of the production plan. Free models, textures, sounds, and
tools will be used when their licenses allow it.

# Main Gameplay Features

## Toy-Scale Exploration

Scale is one of the main reasons the idea works.

A single living room can contain several different routes because the
player is so small. The underside of a sofa, the seat, the backrest, and
the top edge can all function as separate places.

The same applies to tables, shelves, beds, kitchen counters, and outdoor
furniture.

The player should often see places before they know how to reach them.

## RC Car Handling

The car handling takes direct inspiration from the feel of the 1999 game
*Re-Volt*.

The vehicle should feel light, responsive, fast, and slightly unstable
in a way that suits a small RC car. It should react clearly to ramps,
uneven surfaces, collisions, and changes in direction.

Realistic RC simulation is not the goal. The physics should support fun
movement and readable player control.

Acceleration should build quickly enough for short run-ups to matter.
Steering needs to remain responsive without making the car impossible to
control at higher speeds.

The car should bounce, tilt, roll, and react to the environment, but the
player must still feel responsible for what happens.

Air control can be stronger than a realistic RC car would allow because
flips and controlled landings are part of the game.

The car mechanics should make driving across furniture and household
objects enjoyable even when there is no race or timed objective.

## Routes Through Ordinary Objects

The levels should avoid covering the house in obvious videogame ramps.

A few toy ramps make sense, especially in a child’s room or garage, but
most routes should use household objects.

For example:

> Floor $`\rightarrow`$ book $`\rightarrow`$ cardboard box
> $`\rightarrow`$ chair $`\rightarrow`$ table.

A player who understands the car well may skip part of the route with a
larger jump.

That gives skilled players room to experiment without requiring a
separate advanced mode.

## Vertical Exploration

Most rooms need at least one place that looks difficult to reach.

The living room may center on the sofa and bookshelf. The kitchen can
focus on the counter and refrigerator. The bedroom can use the bed,
desk, shelves, and piles of toys.

Getting higher should usually reveal something new. That might be rings,
a shortcut, another route, or a view of an area the player has not
entered yet.

## Stunts

The car can rotate while airborne.

The first trick set should stay small:

- backflip,

- frontflip,

- 360-degree spin,

- barrel roll,

- long jump,

- clean landing.

More advanced moves should only be added if the base controls support
them well.

The stunt system does not need to become a separate simulation. Tricks
are there to make jumps more fun and give players a reason to repeat
routes.

## Collectibles

Rings should guide the player as much as they reward the player.

A line of rings can show a route up a chair. Rings hanging in the air
above a ramp can communicate the intended jump arc. A single ring behind
furniture can hint at a hidden passage.

Random placement would waste that function.

Special collectibles can be harder to reach and may be tied to room
completion or hidden challenges.

# Story Overview

## Setup

The player controls a child’s RC car.

The game does not need a large story around this idea. The house itself
provides enough context.

Toys, books, clothes, boxes, cushions, and improvised ramps suggest that
the car belongs to a child who already treats the house as a place to
play.

The player takes that idea further.

## World

The house should feel occupied without requiring visible human
characters.

A cereal box left near a chair, shoes in the hallway, toys under the
sofa, or books stacked beside a bed can tell the player enough.

Some clutter exists for gameplay. Some exists because a spotless house
would look wrong.

Those two purposes should overlap when possible.

## Moving Through the House

The current room order is:

1.  Living Room

2.  Hallway

3.  Kitchen

4.  Bedroom

5.  Bathroom

6.  Garage

7.  Backyard

8.  Garden or Shed

This order is not fixed yet.

Rooms should connect physically. Opening a door or reaching a new
passage is preferable to returning to a level-select menu.

## Ending

The ending is still TBD.

The current idea is to finish with a difficult route that uses the
skills the player learned during the rest of the game.

The backyard or garage is the best place for this because both allow
larger jumps and longer run-ups.

There is no villain to defeat. Finishing the game should feel like
mastering the car and the house.

# Gameplay Loop

The player enters an area and starts looking around.

Rings or visible landmarks pull them toward simple routes at first.
While moving through the room, they see harder places above or behind
the obvious path.

A typical sequence might begin on the living-room floor. The player
follows rings toward a stack of books, drives onto the coffee table,
then notices a jump from the table to the sofa. Landing on the sofa
reveals another route along the cushions.

The player keeps moving until they find a collectible, shortcut,
challenge, or new area.

Failed jumps should feed back into the same loop quickly. There are no
lives to lose and no long reload screens.

## Moment-to-Moment Play

Most play consists of:

> Drive $`\rightarrow`$ line up a route $`\rightarrow`$ build speed
> $`\rightarrow`$ jump $`\rightarrow`$ control the car in the air
> $`\rightarrow`$ land $`\rightarrow`$ keep moving.

The loop can happen several times within one route.

## Failure and Recovery

The car will often land upside down or fall somewhere awkward. That is
expected.

The player needs a fast reset button.

Reset should return the car to a recent safe position or checkpoint. A
failed jump should cost a few seconds, not several minutes of lost
progress.

# Game Controls

## Control Goals

*Under the Sofa* is designed as a keyboard-controlled PC game.

The controls should stay simple and close to older PC games. WASD
handles normal driving, while a few additional keys cover hopping,
drifting, boosting, resetting, and pausing.

## Keyboard Controls

| **Input** | **Action**                                                   |
|:----------|:-------------------------------------------------------------|
| W         | Accelerate                                                   |
| S         | Brake and reverse                                            |
| A         | Steer left                                                   |
| D         | Steer right                                                  |
| Space     | Small hop                                                    |
| Shift     | Drift                                                        |
| Ctrl      | Boost                                                        |
| Q / E     | Air roll, if separate roll controls are needed after testing |
| R         | Reset the car to a safe position                             |
| Esc       | Pause                                                        |

## Small Hop

Space gives the car a short hop.

The hop helps clear small edges, rough geometry, and low obstacles. It
also gives the player a small amount of control when approaching awkward
surfaces.

The hop should stay low. Ramps remain the main way to gain height or
cross large gaps.

## Drifting

Holding Shift while steering puts the car into a drift.

Drifting helps the player take tighter corners, control the car at
speed, and line up approaches to ramps.

The system should feel loose and arcade-like rather than behave like a
detailed racing simulation.

## Boost

Ctrl activates boost while the boost meter contains enough energy.

Boost gives the car a short burst of extra speed. It is useful for long
jumps, stunt routes, and faster movement through areas the player
already knows.

The exact strength, duration, and recharge rate need prototype testing.

## Air Control

When the car is airborne, normal movement input should influence its
rotation.

The player needs enough control to perform flips and correct small
mistakes, but momentum should still matter.

Q and E can provide separate roll control if WASD alone does not give
enough control for the stunt system.

# Technology

## Engine and Rendering

Development uses Unity 6.5.

The project currently runs on Unity version 6000.5.10f1 with URP 17.5.0.

URP will handle the normal rendering pipeline before the retro effects
are added.

## Input

The project uses Unity’s Input System.

Version 1.20.0 was installed when the project was created.

## Physics

The car will use Rigidbody physics with custom arcade movement.

A realistic vehicle package is not planned.

WheelColliders may be tested, but the game does not depend on using
them. A simpler physics setup may fit the game better and should be
easier to tune.

## Camera

The camera follows behind the car in third person.

Furniture creates a problem here because the camera often needs to work
underneath tables or close to walls. Camera collision and distance
adjustment will need testing early.

A custom camera is acceptable if existing Unity tools become harder to
control than writing a small system.

## Development Tools

The project uses Git, GitHub, and Git LFS.

Codex is connected to the project directory and can help write and
inspect code.

Unity CLI and Unity Pipeline are also connected to the running Unity
Editor. They can be used for Editor automation when doing so saves time.

## Things the Project Does Not Need

The game does not need multiplayer, procedural generation, combat AI,
networking, crafting, dialogue systems, or realistic vehicle simulation.

Those systems would add work without helping the current design.

# Front End

## Startup

The game should open with a short logo or loading screen and then go to
the title screen.

Long intro sequences are not planned.

## Title Screen

The title screen should show the scale of the game immediately.

A good setup would place the camera close to the carpet with the RC car
near or beneath the sofa. Furniture fills most of the background.

The title *Under the Sofa* appears over the scene.

The background can remain live rather than using a static image if
performance allows it.

## Main Menu

The current menu is simple:

- Continue

- New Game

- Options

- Credits

- Quit

Extra menu screens should only be added when the game has a reason for
them.

## Saving

The game should save automatically.

Saved data includes room access, collected rings, special collectibles,
completed challenges, and settings.

Multiple save slots are TBD.

For a short single-player game, one main save may be enough.

## Options

The options menu should stay small and resemble the limited settings
menus found in older PC and console games.

The player can change:

- Master Volume

- Music Volume

- Fullscreen On / Off

- Resolution

The retro rendering style should remain part of the game’s fixed
presentation rather than something the player configures in detail.

Camera behavior, screen effects, and control response should be tuned
well enough that they do not need separate settings.

# Game Camera

The game uses a fixed third-person chase camera behind the RC car.

The player does not control the camera directly. Camera movement should
follow the car automatically so the player can focus on driving, lining
up jumps, drifting, and controlling the car in the air.

The camera should sit fairly close to the car at low speeds. This helps
the RC car feel small and makes furniture appear large around it.

As the car gains speed, the camera gradually moves farther back. This
gives the player a better view of the route ahead and makes high-speed
driving feel faster without changing the controls.

The change in distance should be smooth rather than switching between
fixed camera positions.

A rough camera behavior is:

> Low speed $`\rightarrow`$ close camera
>
> Normal driving $`\rightarrow`$ standard chase distance
>
> High speed or boost $`\rightarrow`$ camera pulls farther back

The camera may also use a small field-of-view change at higher speeds if
testing shows that it improves the sense of speed. Camera distance
should remain the main effect.

## Camera Direction

The camera follows the car’s movement and gradually aligns itself behind
the direction of travel.

It should not snap immediately every time the car turns. A small delay
gives the car room to slide and drift without making the view unstable.

During a drift, the camera can remain slightly behind the car’s movement
direction rather than matching the car’s rotation exactly. This should
make the slide easier to read.

When reversing, the camera should remain predictable rather than rapidly
flipping between directions.

## Jumps and Airborne Movement

During a jump, the camera follows the car without copying its rotation.

A backflip or barrel roll should rotate the car inside the frame while
the horizon remains stable.

The camera may pull back slightly during large jumps so the player can
see the landing area.

Airborne camera behavior should stay simple. The player needs to judge
the landing rather than watch the camera perform its own movement.

## Camera Collision

Furniture and walls should never fully block the player’s view of the
car.

When the normal camera position would pass through a wall, sofa, table,
or another large object, the camera moves closer to the car until the
obstruction is cleared.

Once there is enough room again, it smoothly returns to its normal
distance.

This matters in areas such as underneath the sofa, beneath tables, and
between closely placed furniture.

## Reset Behavior

After the player resets the car, the camera returns to its normal
position behind the vehicle.

The reset should happen quickly and without a long camera transition.

The camera system should require no player input during normal play.

# HUD

The game should avoid covering the screen with permanent information.

A ring counter and short stunt messages are enough during normal play.

Room completion can appear when the player pauses or enters a room.

For example:

    LIVING ROOM

    Rings       17 / 25
    Secrets      2 / 4
    Stunts       3 / 5

Trick messages appear briefly after a successful move.

    BACKFLIP
    +100

    CLEAN LANDING
    x2

The interface should look like part of a late-1990s or early-2000s game
without becoming hard to read.

# Player Vehicle

## Appearance

The RC car should look like a toy from a glance.

Large wheels, a plastic body, simple suspension shapes, and a small
spoiler fit the design. An antenna may help reinforce the older RC look.

The car needs more visual attention than most other assets because the
player sees it for the entire game.

The final model can stay low-poly.

## Handling

The important values are acceleration, top speed, steering response,
grip, mass, center of mass, suspension behavior, jump strength, and air
rotation speed.

No final numbers exist yet.

The first prototype should tune those values by feel.

The car should be stable enough for deliberate movement but loose enough
to create funny crashes.

## Abilities

The base car can accelerate, reverse, steer, brake, drift, perform a
small hop, rotate in the air, and reset when stuck.

The small hop is part of the normal movement system. It helps with low
obstacles and awkward edges but does not replace ramps.

Drifting gives the player more control when approaching corners or
setting up a jump.

The car also has a boost meter. Boost provides a short increase in speed
and can be used for longer jumps, faster routes, and stunt lines.

Boost is part of the base car's control set. The car's capabilities stay
consistent throughout the game rather than changing through progression.

# Player Progression

Most progression comes from the player getting better at controlling the
car.

Routes that feel difficult early in the game should become easier as the
player learns how much speed a jump needs, when to drift, and how the
car behaves in the air.

The car's capabilities do not improve over time. Every required route
must be completable with the base car and the skills the player has
developed.

Progression comes from collecting rings. Each main room has a clearly
communicated ring requirement, and collecting enough rings opens the
route to the next area.

# Failure and Recovery

The car does not have health.

Crashing into a table leg or falling from the sofa should be part of
normal play.

The player fails when the car becomes trapped, falls outside the
playable area, lands in a position it cannot escape from, or fails a
specific challenge.

The reset button handles most of these cases.

The game should remember a recent safe position so the player does not
always return to the room entrance.

A checkpoint is useful after a long climb. Too many checkpoints would
make climbing less meaningful, so placement needs restraint.

# Stunt Scoring

Tricks can award points, but scoring remains secondary to exploration.

Early placeholder values are:

| **Trick**     | **Example Value**   |
|:--------------|:--------------------|
| Backflip      | 100                 |
| Frontflip     | 100                 |
| 360 Spin      | 150                 |
| 720 Spin      | 300                 |
| Barrel Roll   | 150                 |
| Long Jump     | Based on distance   |
| Clean Landing | Bonus or multiplier |

These numbers have no meaning until the stunt system exists.

Points can support optional room challenges such as reaching 2,000
points in one sequence.

Online leaderboards are outside the current scope.

# Rings and Progression

Rings are the main progression collectible, not currency.

The player finds them through normal exploration, difficult climbing
routes, stunt lines, hidden areas, and optional challenges.

Rings also help communicate where the player can go. A trail of rings
may lead toward a ramp, across furniture, or through a difficult jump.

Each main room provides a clearly communicated ring target. Collecting
enough rings opens the route to the next progression area. The target
must be achievable with the base car, without requiring every ring or
any optional challenge.

Rings found in older rooms remain useful for room completion, secrets,
and optional challenges.

## Other Collectibles

Special collectibles may still exist for completion or hidden
challenges.

These should be separate from the main ring progression requirement so
the route to the next area remains easy to understand.

# Game Progression

The player starts in a simple area with enough room to learn how the car
handles.

The living room is the current choice for the first proper room.

From there, the player reaches other parts of the house through doors
and connecting spaces.

| **Stage** | **Area**      | **Main Focus**                                     |
|:----------|:--------------|:---------------------------------------------------|
| 1         | Living Room   | Basic driving, ramps, climbing the sofa.           |
| 2         | Hallway       | Speed and moving between rooms.                    |
| 3         | Kitchen       | Chairs, counters, vertical routes, precise jumps.  |
| 4         | Bedroom       | Toys, soft surfaces, clutter, improvised tracks.   |
| 5         | Bathroom      | Slippery surfaces and tighter routes, if retained. |
| 6         | Garage        | Larger ramps and stunt-focused play.                |
| 7         | Backyard      | Rough ground, long jumps, open routes.             |
| 8         | Garden / Shed | Advanced routes and late-game secrets.             |

The game should let players return to old rooms.

Hidden collectibles, optional challenges, and unexplored routes give
players a reason to come back.

# Gameplay Types

Driving forms the base of every activity.

Platforming comes from using the car to move between surfaces rather
than controlling a character on foot.

Exploration fills the quieter parts of play. Players look for routes,
gaps, collectibles, and ways to get higher.

The game also uses light environmental problem solving. Most of these
problems should be physical and easy to read. The player sees an object
and works out how the car can use it.

Combat, stealth, and enemy encounters are not part of the design.

# World Objects

## Ramps

Some ramps are obvious toy ramps.

Most should come from ordinary objects such as books, cardboard,
cushions, boards, shoes, plates, and tilted furniture.

The shape matters more than the label.

## Physics Objects

Only selected objects should move.

Balls, toy blocks, and a few boxes are good candidates.

Giving every cup, pencil, and book a Rigidbody would create unstable
scenes and more work than the mechanic earns.

Most props stay static.

## Switches and Simple Interactions

A few objects can react when the car touches them.

Examples include floor switches, remote controls, garage buttons, and
toy mechanisms.

These interactions should stay physical and easy to understand.

## Doors and Room Progression

Progression should come from collecting enough rings rather than pushing
objects out of the way.

Each main room has a clear ring target and a visible route to the next
area. When the player reaches the target, the next door, passage, or
other route opens automatically.

Ring placement must reinforce vertical traversal. Required rings should
lead the player across furniture, ramps, jumps, and varied routes; high
locations can hold optional rings, secrets, shortcuts, and challenges.

In the current Living Room prototype, collecting 15 rings opens the
nearby automatic door. The ring layout should guide the player up the
sofa and across the room's vertical routes, so the collection target
reinforces traversal rather than replacing it.

Each progression gate must display its required ring count clearly and
each room must provide enough reachable rings to meet it with the base
car. The player should not need every ring or any optional challenge to
continue.

# Level Design

A room should work as a driving area before it works as a realistic
room.

The furniture still needs to make sense, but exact household realism is
not worth sacrificing a good route.

A sofa can sit a little farther from a wall because the player needs
room behind it. A chair can be angled toward a table because the angle
creates a jump.

That kind of adjustment is fine.

## Clutter

The house should look lived in.

Books, shoes, toys, boxes, clothes, pillows, cups, and other props help
sell the setting.

Clutter also has to leave enough visual space for the player to read the
route. A room filled with hundreds of tiny objects would make navigation
worse.

## Different Routes

Important locations should often have more than one approach.

A simple route can use several safe surfaces.

A player who has learned the car may find a faster jump that skips part
of the climb.

Some routes can be hidden.

The player should be allowed to break the intended path when their
driving skill supports it.

# Levels

## Prototype Test Area

The first development scene exists only to test the car.

It uses primitive shapes and contains flat ground, ramps, platforms,
boxes, and a few awkward surfaces.

No final assets belong here.

The test is simple. If driving through this area is boring, building the
house does not fix the problem.

## Living Room

The living room introduces the scale of the game.

The sofa is the main landmark. The coffee table, TV stand, bookshelf,
rug, chairs, and toys create the rest of the routes.

The first large goal is reaching the sofa.

An easy route may use books or a toy ramp. Another route may require
jumping from the coffee table. Later, the player can attempt to reach
the back of the sofa or jump toward the bookshelf.

The room should teach the main idea without a long tutorial.

## Hallway

The hallway gives the player room to build speed.

The area is narrower and simpler than the living room, so it works well
for time trials or longer jumps.

Door thresholds, shoes, boxes, and narrow furniture can break up the
route.

The hallway also connects rooms, so it should remain useful after the
player has passed through it once.

## Kitchen

The kitchen focuses on height.

The counter is the first major destination.

A chair, stool, box, or tilted object can form the lower part of the
route. Once the player reaches the counter, the sink and appliances
create a second layer of movement.

The top of the refrigerator can act as one of the hardest optional
destinations in the room.

That route should feel excessive when the player first sees it.

## Bedroom

The bedroom leans harder into the playground idea.

The bed creates large soft slopes. Pillows and blankets break up the
surface. Toys, blocks, books, tracks, and cardboard pieces make the room
less orderly than the rest of the house.

A blanket hanging from the bed can form a ramp or tunnel.

The room should feel as if someone was already playing there before the
car arrived.

## Bathroom

The bathroom is optional until the core rooms work.

Tile can create a lower-grip surface. The sink and bathtub provide
narrow raised routes.

The room needs a mechanic of its own to justify production time. If
slippery handling is not fun, the bathroom can be cut without hurting
the rest of the game.

## Garage

The garage supports larger stunts.

Boxes, shelves, tools, boards, workbenches, and stored objects naturally
form ramps and platforms.

## Backyard

The backyard opens the game up.

Grass, dirt, paving, garden furniture, stones, a hose, flower pots,
toys, and a shed create a different kind of driving surface from the
house.

Longer run-ups become possible here.

This is also where the game can place its largest jumps without making
indoor rooms feel absurd.

The backyard should feel like the point where the player can fully use a
car they have learned and improved.

# World Structure

The house should feel connected.

A rough layout is:

    Living Room
        |
    Hallway
       / \
    Kitchen Bedroom
       |      |
    Garage  Bathroom
       |
    Backyard
       |
    Garden / Shed

The final floor plan does not need to match a real house exactly.

Connections should make sense from the car’s point of view and support
good movement between rooms.

# Challenges

Optional challenges reuse the same rooms.

Examples include time trials, stunt-score targets, ring routes,
precision jumps, and reaching a marked location without resetting.

A challenge does not need its own separate level.

The player may see a trail of rings through the kitchen and activate a
timed version of the same route.

This keeps production smaller while giving old areas more use.

# Enemies, Combat, and NPCs

There are no traditional enemies.

There is no combat system.

NPCs are not required either.

The player may hear people in another room or see signs of them in the
environment, but visible characters would create animation, modeling,
and interaction work that the current game does not need.

A vacuum cleaner, fan, moving toy, or sprinkler may behave like a hazard
later.

These are environmental objects, not enemies.

# Art Direction

The game should resemble a late-1990s 3D console game without copying
every limitation of original PlayStation hardware.

Low-poly models and small textures do most of the work.

The renderer can add vertex wobble, low internal resolution, dithering,
fog, and reduced color depth.

Those effects need restraint. If the image becomes difficult to read,
the style is hurting the game.

The visual style and soundtrack should feel like they belong to the same
late-1990s game.

Low-resolution textures, simple 3D geometry, visible rendering
artifacts, electronic synth music, and fast RC movement should work
together.

The target is the feeling of finding an unusual PC or console game from
the late 1990s rather than applying a modern retro filter to a
modern-looking game.

## Models

Props should have simple shapes and clear silhouettes.

Household objects do not need exact polygon limits, but a modern
high-detail sofa beside a chunky PS1 table will look wrong even after
adding a retro shader.

Model choice comes before post-processing.

## Textures

Most textures should sit around 128x128 or 256x256.

Point filtering keeps edges sharp.

Some downloaded assets may arrive with larger textures. Those can be
reduced when doing so brings them closer to the rest of the game.

## Materials

Most surfaces should stay matte.

Strong metallic reflections and smooth modern PBR materials do not fit
the target look.

Plastic on the RC car can have a small amount of shine, but it should
still sit comfortably beside the environment.

## Retro Rendering

The renderer may use:

- low internal resolution,

- vertex snapping,

- affine-style texture warping,

- point-filtered textures,

- reduced color depth,

- dithering,

- fog,

- simple lighting.

CRT scanlines and VHS effects are not a priority.

The game is trying to look like an old 3D game, not like footage of an
old television.

# Asset Strategy

Most 3D art should come from free asset libraries.

The safest approach is to pick one main pack for house structure, one
for furniture, and one outdoor pack.

Other assets should fill specific gaps.

Downloading random free models whenever something is missing will make
the game look inconsistent.

## Custom Work

The RC car deserves custom work if no suitable free model exists.

Rings, simple ramps, batteries, and gameplay markers are small enough to
create without turning Blender into a major part of production.

Everything else should be reused when possible.

## Making Different Packs Fit

Assets from different creators can still work together if their basic
shapes are close enough.

Textures can be reduced to similar sizes. Materials can use the same
shader. Lighting and fog can pull the room together.

Scale matters too.

A chair from one pack and a table from another need to look as if the
same person could use both before they look like they belong in the same
house.

# Music and Sound

The soundtrack is an important part of the identity of *Under the Sofa*.

The main musical direction is late-1990s electronic music inspired by
the trance, acid techno, and synth-heavy sound associated with games
such as *Re-Volt*.

The music should make driving feel faster and give the house a strange
arcade atmosphere. The contrast is intentional. The player is driving
through an ordinary living room or kitchen while energetic electronic
music plays as if the room were a racing circuit.

## Music Direction

The soundtrack should draw from:

- acid techno,

- late-1990s trance,

- old-school electronic game music,

- analog-style synth leads and bass lines,

- repetitive electronic sequences,

- drum-machine percussion,

- breakbeat elements where they fit.

Tracks should sound energetic without becoming too aggressive for
exploration.

The music should work during both slow route-finding and fast stunt
sequences. A track may begin with a simpler synth pattern and build into
stronger percussion rather than demanding constant high-speed action.

The soundtrack should avoid modern festival EDM production. The target
sound is older, simpler, rougher, and more synthetic.

## Relationship Between Music and Gameplay

Music should help turn each room into a driving playground.

A kitchen should still look like a kitchen, but the soundtrack can make
driving along the counters feel like running a race course.

The player may spend several minutes trying the same jump or looking for
a route onto a shelf. The music needs enough repetition to feel hypnotic
rather than distracting during this kind of play.

Large stunt areas such as the garage and backyard can use faster and
heavier tracks.

Smaller indoor rooms can use more restrained trance or synth-based
tracks while keeping the same overall musical identity.

## Room Music

Different rooms may have their own tracks or variations.

Possible direction:

| **Area**    | **Music Direction**                                        |
|:------------|:-----------------------------------------------------------|
| Living Room | Mid-tempo trance with a simple repeating synth melody.     |
| Hallway     | Faster rhythm that suits long acceleration runs.           |
| Kitchen     | Acid-style bass sequence with brighter synth sounds.       |
| Bedroom     | More playful electronic track with toy-like synth tones.   |
| Garage      | Harder acid techno and heavier percussion.                 |
| Backyard    | Faster trance track suited to long jumps and open driving. |

These are direction notes rather than fixed compositions.

## Vehicle Sound

The RC motor should have a clear electronic whine that changes with
speed.

Acceleration should raise the motor pitch. Letting off the throttle
should produce an audible drop.

The sound needs enough character to make the car feel mechanical and
toy-like without becoming irritating during long sessions.

Other vehicle sounds include:

- tire skid,

- suspension impacts,

- hard landings,

- plastic body impacts,

- collisions with furniture,

- turbo sound if turbo is added.

## Surface Sound

Different surfaces should sound different when practical.

Carpet should sound soft and muted. Wood, tile, concrete, and outdoor
ground should have clearer tire and impact sounds.

The system does not need a large number of surface types at the start. A
small set of clearly different materials is enough.

## Collectible Sound

Rings need a short and recognizable pickup sound.

The sound should still work when several rings are collected within one
or two seconds.

Special collectibles should use a related but more noticeable sound so
the player immediately knows they found something uncommon.

# Accessibility

The keyboard controls should stay simple and use familiar PC bindings.

The player should be able to adjust camera sensitivity and invert the
camera if needed.

Screen shake should remain limited by default. The game should avoid
effects that make driving difficult to read.

The retro rendering style is part of the game’s fixed presentation, but
the image still needs to remain clear enough for normal play.

UI text should stay readable at the resolutions supported by the game.

# First Prototype

The first prototype should contain no real art.

A floor, two ramps, a raised platform, boxes, and a primitive car are
enough.

The car needs forward movement, reverse, steering, braking, ramp
launching, air rotation, landing, and reset.

The camera follows behind it.

The purpose of this scene is to find out whether controlling the car is
fun.

A player should be able to spend several minutes driving around without
rings or objectives and still want to try another jump.

If that does not happen, work stays on movement.

The living room comes later.

# Development Plan

Development starts with the vehicle.

Once driving and the camera work, the project can add rings,
checkpoints, stunt detection, and a few physics objects.

The first real environment is the living room.

That room acts as the vertical slice. It should include the intended art
style, real props, ring routes, sofa climbing, at least one hidden
route, and one stunt challenge.

Building the rest of the house before this room works would create a lot
of content around systems that may still change.

After the living room is stable, other rooms can be added one at a time.

The backyard comes later because it needs more environmental assets and
larger terrain.

# Scope Limits

The game does not need every idea that fits an RC car.

Multiplayer, racing opponents, combat, a large story, crafting, NPC
quests, procedural generation, and a detailed inventory are outside the
plan.

Full destruction is also outside the plan.

A few selected boxes or toys can move. The sofa does not need to explode
because the player hit it at full speed.

New mechanics should reuse the car and the rooms whenever possible.

# Development Risks

## The Car Feels Bad

This is the largest risk.

The rest of the game depends on driving.

A good-looking house cannot compensate for weak steering, poor jumps, or
frustrating landings.

The prototype needs to solve this before production moves forward.

## Free Assets Do Not Match

Free assets save modeling time, but mixing too many packs can make the
house look assembled from unrelated games.

The project should reject assets that require too much work to fit.

Changing textures and materials is reasonable. Rebuilding every
downloaded model defeats the point of using free assets.

## Physics Problems

A tiny fast car can react badly to small collider edges.

Simple collision meshes will help.

The car may also need a lower center of mass, custom ground detection,
and some artificial stabilization.

The goal is good control, not pure simulation.

## The House Becomes Too Large

A house sounds like a small environment until every room needs
furniture, clutter, shortcuts, collectibles, collision setup, lighting,
and testing.

Rooms should be cut if they do not bring a different type of play.

The bathroom is the clearest current example. It stays only if its
handling ideas work.

# Remaining Design Questions

Several parts of the game still need answers from prototype testing.

- How high should the small hop be before it starts replacing ramps and
  intended climbing routes?

- How strong should drifting be at low and high speed?

- Should drifting reduce grip, increase turning angle, or use a
  combination of both?

- How should the boost meter recharge?

- Should successful stunts refill boost faster, or should boost recharge
  at a fixed rate?

- How long should one full boost meter last?

- How many rings should each room require to open the next route?

- How many extra rings should each room contain beyond its progression
  requirement?

- Should ring requirements use a total collected count or a separate
  count for each room?

- How many main rooms should the final game contain?

- How should a room clearly communicate its ring requirement and show
  that the next route has opened?

- How difficult should the final challenge be compared with the hardest
  optional routes in the game?

- Should the final challenge require only the car’s base abilities, or
  should it include a separate optional expert route?

# Appendix A: Current Technical Setup

| **Component**      | **Current Setup**     |
|:-------------------|:----------------------|
| Unity              | 6000.5.10f1           |
| Render Pipeline    | URP 17.5.0            |
| Input System       | 1.20.0                |
| Unity Pipeline     | 0.5.0-exp.1           |
| Unity CLI          | 1.0.0-beta.6          |
| Version Control    | Git                   |
| Large File Storage | Git LFS               |
| Remote Repository  | GitHub                |
| Target Platform    | Windows PC            |
| Project Folder     | C:\Games\UnderTheSofa |

# Appendix B: Initial Asset Needs

## Living Room

The first room needs a sofa, coffee table, TV stand, bookshelf, chair,
rug, lamp, books, boxes, cushions, shoes, and several toys.

Most of these can repeat elsewhere in the house.

## Kitchen

The kitchen needs cabinets, counters, chairs, a table, refrigerator,
sink, stove, plates, cups, boxes, and containers.

The chairs and table matter for traversal, so their proportions need
more attention than decorative kitchen props.

## Bedroom

The bedroom needs a bed, desk, chair, shelves, pillows, blankets, books,
toy blocks, stuffed toys, and cardboard boxes.

Toy assets have more room to repeat here because clutter is part of the
room’s identity.

## Backyard

The backyard needs grass, paving, garden furniture, flower pots, plants,
rocks, a hose, toys, a sandbox or similar play object, and a shed.

Trees and bushes should stay simple enough to match the indoor low-poly
style.

# Appendix C: Main Production Rule

The house should be designed from the car’s point of view.

A realistic room can still be boring to drive through.

Furniture, clutter, props, and open floor need to form useful routes.
When realism and movement conflict, movement usually wins.

The player should keep finding places they want to reach.
