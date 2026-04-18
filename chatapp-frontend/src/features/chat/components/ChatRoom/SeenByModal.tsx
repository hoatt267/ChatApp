import type { Participant, ReadReceipt } from "../../types";
import Modal from "../../../../components/ui/Modal";

interface SeenByModalProps {
  isOpen: boolean;
  onClose: () => void;
  receipts: ReadReceipt[] | null;
  participants: Participant[];
}

export default function SeenByModal({
  isOpen,
  onClose,
  receipts,
  participants,
}: SeenByModalProps) {
  if (!receipts || receipts.length === 0) return null;

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Message seen by">
      <div className="max-h-64 overflow-y-auto custom-scrollbar">
        {receipts.map((receipt) => {
          const reader = participants.find((p) => p.userId === receipt.userId);
          if (!reader) return null;

          return (
            <div
              key={receipt.userId}
              className="flex items-center gap-3 p-2 hover:bg-gray-50 rounded-lg transition-colors"
            >
              <img
                src={
                  reader.avatarUrl ||
                  `https://ui-avatars.com/api/?name=${reader.fullName}`
                }
                alt={reader.fullName}
                className="w-10 h-10 rounded-full object-cover border border-gray-100 shadow-sm"
              />
              <div className="flex flex-col">
                <span className="font-semibold text-gray-800 text-sm">
                  {reader.fullName}
                </span>
                <span className="text-xs text-gray-500">
                  {receipt.readAt && !isNaN(new Date(receipt.readAt).getTime())
                    ? new Date(receipt.readAt).toLocaleTimeString([], {
                        hour: "2-digit",
                        minute: "2-digit",
                      })
                    : "Vừa xong"}
                </span>
              </div>
            </div>
          );
        })}
      </div>
    </Modal>
  );
}
