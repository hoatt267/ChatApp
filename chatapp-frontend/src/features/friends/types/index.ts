export interface FriendProfile {
  friendshipId: string;
  userId: string;
  email: string;
  fullName?: string;
  avatarUrl?: string;
  bio?: string;
  status: string; // 'Pending', 'Accepted', 'Blocked'
  isRequester: boolean;
}

export interface FriendshipResponse {
  id: string;
  requesterId: string;
  receiverId: string;
  status: number;
}

export const FriendshipAction = {
  Cancel: "cancel",
  Reject: "reject",
  Unfriend: "unfriend",
} as const;

export type FriendshipAction =
  (typeof FriendshipAction)[keyof typeof FriendshipAction];
