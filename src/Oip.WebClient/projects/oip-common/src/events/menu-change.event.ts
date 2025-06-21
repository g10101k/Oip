import { ContextMenuItemDto } from "../dtos/context-menu-item.dto";

/**
 * Represents an event triggered when a menu item is clicked.
 * @interface MenuChangeEvent
 * @class
 */
export interface MenuChangeEvent {
  /**
   * A string representing a key, used for identification or access.
   */
  key: string;
  /**
   * Indicates whether a route event has occurred.
   * @type {boolean}
   */
  routeEvent?: boolean;
  /**
   * Represents a single item within a context menu.  Contains data necessary to display the item
   * and execute its associated action.
   */
  item: ContextMenuItemDto;
}
