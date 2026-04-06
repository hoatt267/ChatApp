import styled from "styled-components";

export const PageWrapper = styled.div.attrs({
  className: "min-h-screen flex items-center justify-center bg-gray-100",
})``;

export const FormCard = styled.div.attrs({
  className: "bg-white p-8 rounded-xl shadow-lg max-w-md w-full",
})``;

export const SubmitButton = styled.button.attrs<{ $loading: boolean }>(
  (props) => ({
    className: `w-full text-white font-bold py-2 px-4 rounded-lg transition duration-200 flex justify-center items-center ${
      props.$loading
        ? "bg-blue-400 cursor-not-allowed"
        : "bg-blue-600 hover:bg-blue-700"
    }`,
  }),
)<{ $loading: boolean }>``;
