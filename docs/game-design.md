# Game design

## Story
The player inherits a small, empty store. All that was left behind is a kettle, a jar of instant coffee, a stack of take-away cups and a small counter. They open a tiny coffee stand with what they have, and grow it into a proper coffee shop with the money they earn.

## Intro (new game only)
1. Aunt Bea walks in and welcomes the player to the shop they've inherited. The community group used it for meetings, and there may be coffee things in the boxes.
2. The player clicks the boxes and the kettle, coffee jar and cups pop out onto the brew counter.
3. Aunt Bea asks for a coffee. Making it is the tutorial: a hint at the top of the screen always shows the next step, and she has unlimited patience. It's on the house.
4. She says it's not half bad and suggests turning the place into a coffee shop. The player agrees, she leaves, and Day 1 begins.

Continuing a save skips the intro. The dialogue lives in `resources/dialogue/`.

## Aunt Bea's second visit (before Day 2)
When the player starts Day 2, Aunt Bea comes back to ask how the first day went and brings a tip jar, which pops onto the service counter. She explains tipping and, unless the player has already bought **Clean the Place Up**, suggests cleaning the place up because customers are more patient (and so tip more) in a clean shop. Then she leaves and the Day 2 banner shows. If the player quits during the visit, it plays again next time. The tip jar is on the counter from then on.

## Money and tips
- Money is kept in whole cents and shown as dollars and cents, e.g. `$ 2.00`. Drink prices and upgrade costs are set in dollars in steps of 5 cents.
- From Day 2, each served customer tips up to 50% of the drink's price, scaled by how much patience they had left (full patience means a $ 2.00 drink earns $ 3.00 in all; no patience left means no tip). The 50% is the Shop's *Max Tip Percent*. Tips are rounded to the nearest 5 cents, so balances are always multiples of 5 cents.
- What each customer paid, tip included (e.g. "$ 2.45"), floats up over them when they're served, and the day summary shows how much of the earnings came from tips.
- Customers with unlimited patience (Aunt Bea) never tip.

## Core loop (per day)
1. Customers come in through the door in the back wall and take a free spot along the service counter while the shop is open.
2. Each customer thinks for a moment, then shows a "ready" bubble.
3. The player clicks any ready customer to take their order, makes the drink at the brew counter, and serves it to any customer who ordered that drink.
4. Patience only runs once a customer is ready to order, and gets a small boost when their order is taken. Customers who run out leave without paying.
5. At closing time the last customers are served, the day's results are shown, and progress is saved.

## Making instant coffee
Each step is a click on a brew counter station; the barista walks over and does it.

| Step | Station | Needs |
|---|---|---|
| Lift the kettle | Kettle base | Empty hands, empty kettle |
| Fill it (a few seconds; the barista waits) | Sink | Holding the empty kettle |
| Put it back and boil (the barista is free meanwhile) | Kettle base | Holding the full kettle |
| Grab a cup (click again to put an empty one back) | Cup stack | Empty hands |
| Add granules | Coffee jar | Holding an empty cup |
| Pour hot water | Kettle base | Kettle boiled, holding a cup of granules |

The starting kettle only holds one cup, so every coffee needs a fresh fill and boil. Bigger kettles are Equipment upgrades. A cup of granules or an unwanted drink can be tipped out at the sink.

## Difficulty
Each day brings more customers: 4 on Day 1, then 2 more each day, up to 30. The gap between arrivals is the day length shared out between them (about 30 s on Day 1), but never less than 4 s on average, and each gap varies by ±30%. If every spot at the counter is full, the next customer waits and arrives later. Once everyone due that day has been served or given up, the clock runs 10× faster to closing time (the clock shows `>>`). The summary shows how many of the day's customers were served. All of these numbers are on the Shop's *Customers per day* exports.

## Progression
Money earned is spent between days on the upgrades page, opened from the end-of-day summary. Each upgrade is bought once; tiers chain with *Requires* (comfy shoes → running shoes). Bonuses from upgrades with the same effect add up. The upgrades live in `resources/upgrades/`, listed in `catalog.tres`.

| Tab | Upgrade | Cost | Effect |
|---|---|---|---|
| Barista | Comfy Shoes | $15 | Walk +25% |
| Barista | Running Shoes (needs Comfy Shoes) | $40 | Walk +25% |
| Barista | Warm Smile | $25 | Patience +20% |
| Equipment | Better Tap | $15 | Kettle fills twice as fast |
| Equipment | Rapid-Boil Element | $30 | Kettle boils 50% faster |
| Equipment | Bigger Kettle | $20 | +1 cup (2 total) |
| Equipment | Family Kettle (needs Bigger Kettle) | $50 | +2 cups (4 total) |
| Shop | Clean the Place Up | $10 | Removes the stains, cobwebs, boxes and smashed picture, and cleans the window; patience +10% |

Still to come: coffee machines and new drinks (Equipment), tables, a bigger shop, and a decorating feature where the player places plants and other decor themselves (Shop).

Longer-term categories:

| Category | Examples | Affects |
|---|---|---|
| Space | Knock through into the unused rooms; longer queue; seating | Queue length, layout |
| Equipment | Kettle → drip machine → espresso machine; second brew station | Brew speed, which drinks can be made |
| Ingredients | Instant → ground → fresh beans; milk; syrups | Menu, prices, customer happiness |
| Training | Faster barista; better latte art | Barista speed, tips |
| Decor | Paint, plants, lighting | Cosiness, customer patience |

## Starting store (day 1)
- Small single room in the centre of the screen. The rest of the building is dark, unrenovated space.
- A worn service counter runs up the middle of the room from the front: customers on the right, the barista on the left.
- A brew counter against the barista's back (left) wall holds a sink, the kettle on its base, a jar of instant coffee and a stack of take-away cups.
- The door is in the back wall, on the customers' side.
- Menu: Instant Coffee only ($ 2.00). The kettle holds one cup.
- Short queue (4). Day 1 has 4 customers, spread across the day.

## Decor that tells the story
Props in the starting store show it's inherited and neglected. Everything in the shop scene's `Mess` node goes when the player buys **Clean the Place Up**:

| Prop | After cleaning up |
|---|---|
| Dusty window with a taped crack | Clean window |
| Smashed picture hanging crooked | Taken down |
| Damp stain on the back wall | Gone |
| Coffee stains on the floor | Gone |
| Cobwebs (by the window and in the front corner) | Gone |
| Cardboard boxes the coffee things came in | Cleared out |

The handmade cardboard OPEN/CLOSED sign (appears from Day 1; flips to CLOSED at closing time) stays, and could be replaced with a proper shop sign later. `old_photo.svg` (the previous owner) and `healthy_plant.svg` aren't used for now; they're kept for a story hook and for decorating.

## Future unlocks already in the project
Espresso, Latte and Cappuccino recipes exist in `resources/drinks/` but are not on the starting menu. The coffee machine and large counter art in `assets/art/shop/` are for later upgrades.
