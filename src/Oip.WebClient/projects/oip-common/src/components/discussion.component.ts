import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, Input, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { BadgeModule } from 'primeng/badge';
import { ChartModule } from 'primeng/chart';
import { AvatarModule } from 'primeng/avatar';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { MenuModule } from 'primeng/menu';
import { Textarea } from 'primeng/textarea';
import { ToggleSwitchModule } from 'primeng/toggleswitch';

@Component({
  selector: 'chat-app',
  standalone: true,
  imports: [CommonModule, RouterModule, ChartModule, ToggleSwitchModule, BadgeModule, FormsModule, AvatarModule, ButtonModule, InputTextModule, MenuModule, Textarea],
  template: `
    <div class="flex-1 overflow-y-auto flex flex-col gap-8 py-8 px-6">
      @for (message of chatMessages; track message) {
        <div class="flex items-start min-w-64 w-fit max-w-[60%]"
             [ngClass]="{ 'ml-auto mr-0 flex-row-reverse': message.type === 'sent' }">
          <div
            class="flex items-center gap-2 sticky top-0 transition-all"
            [ngClass]="{ 'flex-row-reverse': message.type === 'sent' }">
            <p-avatar
              [image]="message.image"
              [label]="!message.image ? message.capName : ''"
              [ngClass]="{'bg-primary-100 text-primary-950': !message.image }"
              class="w-10 h-10 text-sm font-medium p-link"
              shape="circle"
            />
            <div>
              <svg
                [ngClass]="{
                                    'fill-surface-100 dark:fill-surface-800': message.type === 'received',
                                    'fill-primary rotate-180': message.type !== 'received'
                                }"
                class=""
                xmlns="http://www.w3.org/2000/svg"
                width="7"
                height="11"
                viewBox="0 0 7 11"
                fill="none"
              >
                <path
                  d="M1.79256 7.09551C0.516424 6.31565 0.516426 4.46224 1.79256 3.68238L7 0.500055L7 10.2778L1.79256 7.09551Z"/>
              </svg>
            </div>
          </div>
          <div
            [ngClass]="message.type === 'received' ? 'flex-1 bg-surface-100 dark:bg-surface-800 px-2 py-1 rounded-lg' : 'flex-1 bg-primary px-2 py-1 rounded-lg'">
            <p
              [ngClass]="message.type === 'received' ? 'text-color leading-6 mb-0' : 'text-primary-contrast leading-6 mb-0'">
              {{ message.message }}
            </p>
            <div *ngIf="message.attachment"
                 :class="message.type === 'received' ? 'bg-surface-200 dark:bg-surface-700' : 'bg-primary-emphasis'"
                 class="mt-2 w-full rounded-lg mb-0.5 hover:opacity-75 transition-all">
              <img class="w-full h-auto block cursor-pointer" [src]="message.attachment" alt="Message Image"/>
            </div>
          </div>
        </div>

      }

    </div>
    <div class="p-4 border-t border-surface flex items-end justify-between gap-2">
      <div class="flex items-end gap-1 flex-1">
        <p-button icon="pi pi-face-smile" text/>
        <p-button icon="pi pi-paperclip" text/>
        <textarea pTextarea class="ml-1 flex-1 border-0 shadow-none max-h-32 min-h-9 bg-emphasis overflow-auto"
                  [autoResize]="true" rows="1" placeholder="Write your message..."></textarea>
      </div>
      <p-button icon="pi pi-send"/>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DiscussionComponent implements OnInit {
  chatMessages: any;
  @Input() objectTypeId: number;
  @Input() objectId: number;
  ngOnInit() {
    this.chatMessages = [
      {
        id: 1,
        attachment: '',
        name: '',
        image: '',
        capName: 'OS',
        type: 'received',
        message: "Awesome! What's the standout feature?"
      },
      {
        id: 2,
        attachment: '',
        name: '',
        image: 'https://www.primefaces.org/cdn/primevue/images/landing/apps/avatar8.png',
        capName: 'A',
        type: 'received',
        message: 'PrimeNG rocks! Simplifies UI dev with versatile components.'
      },
      {
        id: 3,
        attachment: '',
        name: '',
        image: 'https://www.primefaces.org/cdn/primevue/images/landing/apps/avatar11.jpg',
        capName: 'A',
        type: 'received',
        message: 'Intriguing! Tell us more about its impact.'
      },
      {
        id: 4,
        attachment: 'https://www.primefaces.org/cdn/primevue/images/landing/apps/message-image.png',
        name: '',
        image: 'https://www.primefaces.org/cdn/primevue/images/landing/apps/avatar2.png',
        capName: 'A',
        type: 'received',
        message: "It's design-neutral and compatible with Tailwind. Features accessible, high-grade components!"
      },
      {
        id: 5,
        attachment: '',
        name: '',
        image: 'https://www.primefaces.org/cdn/primevue/images/landing/apps/avatar5.png',
        capName: 'A',
        type: 'sent',
        message: 'Customizable themes, responsive design – UI excellence!'
      },
      {
        id: 6,
        attachment: '',
        name: '',
        image: 'https://www.primefaces.org/cdn/primevue/images/landing/apps/avatar8.png',
        capName: 'A',
        type: 'received',
        message: 'Love it! Fast-tracking our development is key.'
      },
      {
        id: 7,
        attachment: '',
        name: '',
        image: 'https://www.primefaces.org/cdn/primevue/images/landing/apps/avatar6.png',
        capName: 'A',
        type: 'received',
        message: 'Documentation rocks too – smooth integration for all.'
      },
      {
        id: 8,
        attachment: '',
        name: '',
        image: 'https://www.primefaces.org/cdn/primevue/images/landing/apps/avatar5.png',
        capName: 'B',
        type: 'sent',
        message: 'The flexibility and ease of use are truly impressive. Have you explored the new components?'
      },
      {
        id: 9,
        attachment: '',
        name: '',
        image: 'https://www.primefaces.org/cdn/primevue/images/landing/apps/avatar12.jpg',
        capName: 'C',
        type: 'received',
        message: 'Absolutely, the new calendar component has saved us a ton of development time!'
      },
      {
        id: 10,
        attachment: '',
        name: '',
        image: 'https://www.primefaces.org/cdn/primevue/images/landing/apps/avatar13.jpg',
        capName: 'D',
        type: 'received',
        message: "And the accessibility features are top-notch. It's great to see a library focusing on inclusivity."
      },
      {
        id: 11,
        attachment: 'https://www.primefaces.org/cdn/primevue/images/landing/apps/message-image.png',
        name: '',
        image: 'https://www.primefaces.org/cdn/primevue/images/landing/apps/avatar5.png',
        capName: 'E',
        type: 'sent',
        message: "I couldn't agree more. Plus, the documentation is incredibly thorough, which makes onboarding new team members a breeze."
      },
      {
        id: 12,
        attachment: '',
        name: '',
        image: 'https://www.primefaces.org/cdn/primevue/images/landing/apps/avatar6.png',
        capName: 'F',
        type: 'received',
        message: 'Do you have any tips for optimizing performance when using multiple complex components?'
      },
      {
        id: 13,
        attachment: '',
        name: '',
        image: 'https://www.primefaces.org/cdn/primevue/images/landing/apps/avatar11.jpg',
        capName: 'G',
        type: 'received',
        message: 'Yes! Lazy loading and code splitting can make a huge difference, especially in larger applications.'
      },
      {
        id: 14,
        attachment: '',
        name: '',
        image: '',
        capName: 'HS',
        type: 'received',
        message: "I've also found that leveraging the component's internal state management capabilities can help streamline data flow and improve performance."
      },
      {
        id: 15,
        attachment: '',
        name: '',
        image: 'https://www.primefaces.org/cdn/primevue/images/landing/apps/avatar5.png',
        capName: 'H',
        type: 'sent',
        message: "That's great advice. It's amazing how much detail and thought has gone into making PrimeNG such a powerful tool for developers."
      }
    ];


  }
}
