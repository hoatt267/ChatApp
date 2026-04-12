export interface Participant {
  userId: string;
  fullName: string;
  avatarUrl: string;
}

export interface Conversation {
  id: string;
  title?: string;
  isGroup: boolean;
  createdAt: string;
  participants: Participant[];
  lastMessage?: Message;
}

export const MessageType = {
  Text: 0,
  Image: 1,
  Video: 2,
  Audio: 3,
  Document: 4,
  System: 5,
} as const;
export type MessageTypeValue = (typeof MessageType)[keyof typeof MessageType];

export interface ReadReceipt {
  userId: string;
  readAt: string;
}

export interface Message {
  id: string;
  senderId: string;
  senderName: string;
  senderAvatarUrl: string;
  conversationId: string;
  content: string;
  createdAt: string;
  readBy: ReadReceipt[];
  type: MessageTypeValue;
  fileUrl?: string;
  fileName?: string;
  isOptimistic?: boolean;
  progress?: number;
}
