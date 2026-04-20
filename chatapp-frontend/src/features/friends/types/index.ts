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

export const FriendshipStatus = {
  Pending: 0,
  Accepted: 1,
  Blocked: 2,
  None: 3,
};

export type FriendshipStatus =
  (typeof FriendshipStatus)[keyof typeof FriendshipStatus];

export interface DiscoverUser {
  userId: string;
  fullName: string;
  email: string;
  avatarUrl?: string;
  bio?: string;
  friendshipStatus: FriendshipStatus;
}
