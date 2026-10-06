# Game design

## Story
The player inherits a small, empty store. All that was left behind is a kettle, a jar of instant coffee, a stack of take-away cups and a small counter. They open a tiny coffee stand with what they have, and grow it into a proper coffee shop with the money they earn.

## Core loop (per day)
1. Customers come in through the door in the back wall and take a free spot along the service counter while the shop is open.
2. Each customer thinks for a moment, then shows a "ready" bubble.
3. The player clicks any ready customer to take their order, makes the drink at the brew counter, and serves it to any customer who ordered that drink.
4. Patience only runs once a customer is ready to order, and gets a small boost when their order is taken. Customers who run out leave without paying.

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

The starting kettle only holds one cup, so every coffee needs a fresh fill and boil. A bigger kettle is a planned Equipment upgrade. A cup of granules or an unwanted drink can be tipped out at the sink.
5. At closing time the last customers are served, the day's results are shown, and progress is saved.

## Progression
Money earned is spent between days on upgrades. Planned categories:

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
- Menu: Instant Coffee only ($2). The kettle holds one cup.
- Short queue (4) and a customer every ~5 s.

## Decor that tells the story
Props in the starting store show it's inherited and a bit neglected. Several are meant to be improved by Decor upgrades:

| Prop | Upgrade idea |
|---|---|
| Dusty window with a taped crack | Clean / replace the window |
| Wilted plant | Revive it, then add more plants |
| Cardboard boxes | Clear them out to free floor space |
| Cobweb | Deep clean |
| Faded photo of the previous owner | Story hook (who left the store?) |
| Handmade cardboard OPEN sign | Replace with a proper shop sign |

## Future unlocks already in the project
Espresso, Latte and Cappuccino recipes exist in `resources/drinks/` but are not on the starting menu. The coffee machine and large counter art in `assets/art/shop/` are for later upgrades.
