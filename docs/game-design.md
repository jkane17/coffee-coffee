# Game design

## Story
The player inherits a small, empty store. All that was left behind is a kettle, a jar of instant coffee, a stack of take-away cups and a small counter. They open a tiny coffee stand with what they have, and grow it into a proper coffee shop with the money they earn.

## Core loop (per day)
1. Customers arrive and queue at the counter while the shop is open.
2. The customer at the front orders a drink from the menu.
3. The player clicks the brew station to make it, then clicks the customer to serve it.
4. Customers who wait too long leave without paying.
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
- Small worn counter, a kettle, a stack of take-away cups.
- Menu: Instant Coffee only ($2, 4 s to boil).
- Short queue (4) and a customer every ~5 s.

## Future unlocks already in the project
Espresso, Latte and Cappuccino recipes exist in `resources/drinks/` but are not on the starting menu. The coffee machine and large counter art in `assets/art/shop/` are for later upgrades.
